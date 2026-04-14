using System.Net;
using WikiRacer.Application.Abstractions.Clock;
using WikiRacer.Application.Abstractions.Identifiers;
using WikiRacer.Application.Abstractions.Lobbies;
using WikiRacer.Application.Abstractions.Matches;
using WikiRacer.Application.Abstractions.Sessions;
using WikiRacer.Application.Abstractions.Tokens;
using WikiRacer.Domain.Languages;
using WikiRacer.Domain.Lobbies;
using WikiRacer.Domain.Players;

namespace WikiRacer.Application.Lobbies;

public sealed class LobbyService(
    ILobbyRepository lobbyRepository,
    IMatchRepository matchRepository,
    IPlayerSessionStore playerSessionStore,
    IPublicLobbyIdGenerator publicLobbyIdGenerator,
    ISessionTokenFactory sessionTokenFactory,
    IClock clock) : ILobbyService
{
    private static readonly TimeSpan LobbyLifetime = TimeSpan.FromHours(4);
    private const string ValidationFailed = "validation_failed";
    private const string LobbyFull = "lobby_full";
    private const string LobbyNotJoinable = "lobby_not_joinable";
    private const string LobbyNotFound = "lobby_not_found";
    private const string PlayerNotFound = "player_not_found";
    private const string ReconnectRejected = "reconnect_rejected";
    private const string UnauthorizedAction = "unauthorized_action";

    public async Task<CreateLobbyResult> CreateAsync(CreateLobbyCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var lobby = new Lobby(
            LobbyId.New(),
            publicLobbyIdGenerator.Create(),
            new LobbySettings(
                new WikipediaLanguage(command.Language),
                new WikipediaLanguage(command.UiLanguage),
                command.PlayerCap,
                command.TimeLimitSeconds),
            now,
            now.Add(LobbyLifetime));

        var hostPlayer = lobby.AddHost(PlayerId.New(), command.DisplayName);
        var session = CreateSession(lobby, hostPlayer.PlayerId, hostPlayer.DisplayName);

        await lobbyRepository.AddAsync(lobby, cancellationToken);
        await playerSessionStore.SaveAsync(session, cancellationToken);

        return new CreateLobbyResult(lobby, session);
    }

    public async Task<LobbySnapshotResult> GetByPublicIdAsync(string publicLobbyId, CancellationToken cancellationToken)
    {
        var lobby = await GetLobbyOrThrowAsync(publicLobbyId, cancellationToken);

        lock (lobby)
        {
            NormalizeCountdown(lobby, clock.UtcNow);
        }

        return new LobbySnapshotResult(lobby);
    }

    public async Task<JoinLobbyResult> JoinAsync(JoinLobbyCommand command, CancellationToken cancellationToken)
    {
        var lobby = await GetLobbyOrThrowAsync(command.PublicLobbyId, cancellationToken);

        lock (lobby)
        {
            if (!string.IsNullOrWhiteSpace(command.ReconnectToken))
            {
                return Reconnect(lobby, command.ReconnectToken!, cancellationToken).GetAwaiter().GetResult();
            }

            if (string.IsNullOrWhiteSpace(command.DisplayName))
            {
                throw new LobbyOperationException(ValidationFailed, "Display name is required.", (int)HttpStatusCode.BadRequest);
            }

            if (!lobby.IsJoinable(clock.UtcNow))
            {
                if (lobby.Status == LobbyStatus.InMatch)
                {
                    return JoinActiveMatch(lobby, command.DisplayName, cancellationToken);
                }

                ThrowNotJoinable(lobby);
            }

            var player = lobby.AddPlayer(PlayerId.New(), command.DisplayName);
            var session = CreateSession(lobby, player.PlayerId, player.DisplayName);
            ApplyCountdownState(lobby, clock.UtcNow);
            playerSessionStore.SaveAsync(session, cancellationToken).GetAwaiter().GetResult();

            return new JoinLobbyResult(lobby, session, WasReconnect: false);
        }
    }

    public async Task<UpdateLobbyLanguageResult> UpdateLanguageAsync(UpdateLobbyLanguageCommand command, CancellationToken cancellationToken)
    {
        var lobby = await GetLobbyOrThrowAsync(command.PublicLobbyId, cancellationToken);
        var playerId = ParsePlayerId(command.PlayerId);
        var language = new WikipediaLanguage(command.Language);

        lock (lobby)
        {
            try
            {
                lobby.UpdateLanguage(playerId, language, language);
                ApplyCountdownState(lobby, clock.UtcNow);
            }
            catch (InvalidOperationException exception) when (exception.Message.Contains("host", StringComparison.OrdinalIgnoreCase))
            {
                throw new LobbyOperationException(UnauthorizedAction, exception.Message, (int)HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException exception) when (exception.Message.Contains("part of the lobby", StringComparison.OrdinalIgnoreCase))
            {
                throw new LobbyOperationException(PlayerNotFound, exception.Message, (int)HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException exception)
            {
                throw new LobbyOperationException(LobbyNotJoinable, exception.Message, (int)HttpStatusCode.Conflict);
            }
        }

        return new UpdateLobbyLanguageResult(lobby);
    }

    private async Task<JoinLobbyResult> Reconnect(Lobby lobby, string reconnectToken, CancellationToken cancellationToken)
    {
        var session = await playerSessionStore.GetByReconnectTokenAsync(reconnectToken, cancellationToken);

        if (session is null || session.PublicLobbyId != lobby.PublicId)
        {
            throw new LobbyOperationException(ReconnectRejected, "Reconnect token is invalid for this lobby.", (int)HttpStatusCode.Unauthorized);
        }

        var player = lobby.FindPlayer(session.PlayerId);

        if (player is null)
        {
            throw new LobbyOperationException(PlayerNotFound, "Player session is no longer valid for this lobby.", (int)HttpStatusCode.NotFound);
        }

        lobby.MarkPlayerConnected(session.PlayerId);
        ApplyCountdownState(lobby, clock.UtcNow);

        var refreshedSession = session with
        {
            ConnectionToken = sessionTokenFactory.Create()
        };

        await playerSessionStore.SaveAsync(refreshedSession, cancellationToken);

        return new JoinLobbyResult(lobby, refreshedSession, WasReconnect: true);
    }

    private JoinLobbyResult JoinActiveMatch(Lobby lobby, string displayName, CancellationToken cancellationToken)
    {
        if (lobby.IsFull)
        {
            ThrowNotJoinable(lobby);
        }

        var activeMatchId = lobby.ActiveMatchId;

        if (activeMatchId is null)
        {
            throw new LobbyOperationException(LobbyNotJoinable, "Lobby match state is not available.", (int)HttpStatusCode.Conflict);
        }

        var match = matchRepository.GetByIdAsync(activeMatchId.Value, cancellationToken).GetAwaiter().GetResult();

        if (match is null)
        {
            throw new LobbyOperationException(LobbyNotJoinable, "Lobby match state is not available.", (int)HttpStatusCode.Conflict);
        }

        lock (match)
        {
            try
            {
                if (match.Status != WikiRacer.Domain.Matches.MatchStatus.InProgress)
                {
                    throw new InvalidOperationException("Match is not accepting players.");
                }

                var playerId = PlayerId.New();
                var player = lobby.AddPlayerDuringMatch(playerId, displayName);
                match.AddPlayer(player.PlayerId, player.DisplayName, player.IsConnected);

                var session = CreateSession(lobby, player.PlayerId, player.DisplayName);
                playerSessionStore.SaveAsync(session, cancellationToken).GetAwaiter().GetResult();

                return new JoinLobbyResult(lobby, session, WasReconnect: false);
            }
            catch (InvalidOperationException exception) when (exception.Message.Contains("full", StringComparison.OrdinalIgnoreCase))
            {
                throw new LobbyOperationException(LobbyFull, "Lobby is full.", (int)HttpStatusCode.Conflict);
            }
            catch (InvalidOperationException exception)
            {
                throw new LobbyOperationException(LobbyNotJoinable, exception.Message, (int)HttpStatusCode.Conflict);
            }
        }
    }

    private static void ThrowNotJoinable(Lobby lobby)
    {
        var errorCode = lobby.IsFull ? LobbyFull : LobbyNotJoinable;
        var message = lobby.IsFull ? "Lobby is full." : "Lobby is not joinable.";
        throw new LobbyOperationException(errorCode, message, (int)HttpStatusCode.Conflict);
    }

    private async Task<Lobby> GetLobbyOrThrowAsync(string publicLobbyId, CancellationToken cancellationToken)
    {
        PublicLobbyId parsedPublicLobbyId;

        try
        {
            parsedPublicLobbyId = new PublicLobbyId(publicLobbyId);
        }
        catch (ArgumentException exception)
        {
            throw new LobbyOperationException(ValidationFailed, exception.Message, (int)HttpStatusCode.BadRequest);
        }

        var lobby = await lobbyRepository.GetByPublicIdAsync(parsedPublicLobbyId, cancellationToken);

        if (lobby is null)
        {
            throw new LobbyOperationException(LobbyNotFound, "Lobby was not found.", (int)HttpStatusCode.NotFound);
        }

        if (lobby.ExpiresAtUtc <= clock.UtcNow)
        {
            lock (lobby)
            {
                lobby.MarkExpired();
            }

            throw new LobbyOperationException(LobbyNotJoinable, "Lobby has expired.", (int)HttpStatusCode.Gone);
        }

        return lobby;
    }

    private LobbyPlayerSession CreateSession(Lobby lobby, PlayerId playerId, string displayName)
    {
        return new LobbyPlayerSession(
            lobby.Id,
            lobby.PublicId,
            playerId,
            displayName,
            sessionTokenFactory.Create(),
            sessionTokenFactory.Create());
    }

    private PlayerId ParsePlayerId(string playerId)
    {
        if (!Guid.TryParse(playerId, out var value))
        {
            throw new LobbyOperationException(ValidationFailed, "Player id is invalid.", (int)HttpStatusCode.BadRequest);
        }

        return new PlayerId(value);
    }

    private static void ApplyCountdownState(Lobby lobby, DateTimeOffset now)
    {
        NormalizeCountdown(lobby, now);
    }

    private static void NormalizeCountdown(Lobby lobby, DateTimeOffset now)
    {
        if (lobby.CountdownEndsAtUtc is not null && lobby.CountdownEndsAtUtc <= now)
        {
            lobby.SetCountdown(null);
        }
    }
}

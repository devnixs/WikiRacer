using System.Net;
using WikiRacer.Application.Abstractions.Articles;
using WikiRacer.Application.Abstractions.Clock;
using WikiRacer.Application.Abstractions.Lobbies;
using WikiRacer.Application.Abstractions.Matches;
using WikiRacer.Application.Lobbies;
using WikiRacer.Domain.Lobbies;
using WikiRacer.Domain.Matches;
using WikiRacer.Domain.Players;

namespace WikiRacer.Application.Matches;

public sealed class MatchService(
    ILobbyRepository lobbyRepository,
    IMatchRepository matchRepository,
    IWikipediaArticleClient wikipediaArticleClient,
    IClock clock)
{
    public async Task<Match> StartAsync(StartMatchCommand command, CancellationToken cancellationToken)
    {
        var lobby = await lobbyRepository.GetByPublicIdAsync(new PublicLobbyId(command.PublicLobbyId), cancellationToken);

        if (lobby is null)
        {
            throw new LobbyOperationException("lobby_not_found", "Lobby was not found.", (int)HttpStatusCode.NotFound);
        }

        var requestedBy = ParsePlayerId(command.RequestedByPlayerId);

        lock (lobby)
        {
            if (lobby.ActiveMatchId is not null)
            {
                throw new LobbyOperationException("lobby_not_joinable", "Lobby is already in a match.", (int)HttpStatusCode.Conflict);
            }

            if (!lobby.CanStartCountdown())
            {
                throw new LobbyOperationException("validation_failed", "All connected players must be ready and both articles must be selected.", (int)HttpStatusCode.BadRequest);
            }

            var match = new Match(
                MatchId.New(),
                lobby.Id,
                lobby.PublicId.Value,
                lobby.Settings.Language.Value,
                lobby.Settings.StartArticle!,
                lobby.Settings.TargetArticle!,
                lobby.Players.Select(player => new MatchPlayer(player.PlayerId, player.DisplayName, player.IsConnected)),
                clock.UtcNow);

            lobby.StartMatch(requestedBy, match.Id);
            matchRepository.AddAsync(match, cancellationToken).GetAwaiter().GetResult();

            return match;
        }
    }

    public async Task<Match> GetByLobbyPublicIdAsync(string publicLobbyId, CancellationToken cancellationToken)
    {
        var lobby = await lobbyRepository.GetByPublicIdAsync(new PublicLobbyId(publicLobbyId), cancellationToken);

        if (lobby is null)
        {
            throw new LobbyOperationException("lobby_not_found", "Lobby was not found.", (int)HttpStatusCode.NotFound);
        }

        var match = await matchRepository.GetByLobbyIdAsync(lobby.Id, cancellationToken);

        if (match is null)
        {
            throw new LobbyOperationException("match_not_found", "Match was not found.", (int)HttpStatusCode.NotFound);
        }

        return match;
    }

    public async Task<Match> ReportProgressAsync(ReportMatchProgressCommand command, CancellationToken cancellationToken)
    {
        var match = await GetMatchOrThrowAsync(command.MatchId, cancellationToken);
        var playerId = ParsePlayerId(command.PlayerId);
        var resolved = await wikipediaArticleClient.ResolveAsync(new WikiRacer.Domain.Languages.WikipediaLanguage(match.Language), command.CurrentArticleTitle, cancellationToken);

        if (resolved is null)
        {
            throw new LobbyOperationException("validation_failed", "Article could not be resolved to a playable Wikipedia page.", (int)HttpStatusCode.BadRequest);
        }

        lock (match)
        {
            try
            {
                match.ReportProgress(playerId, resolved.Title, command.ReportedAtUtc);
            }
            catch (InvalidOperationException exception)
            {
                throw new LobbyOperationException("player_not_found", exception.Message, (int)HttpStatusCode.NotFound);
            }
        }

        return match;
    }

    public async Task<Match> AbandonAsync(AbandonMatchCommand command, CancellationToken cancellationToken)
    {
        var match = await GetMatchOrThrowAsync(command.MatchId, cancellationToken);
        var playerId = ParsePlayerId(command.PlayerId);

        lock (match)
        {
            try
            {
                match.Abandon(playerId);
            }
            catch (InvalidOperationException exception)
            {
                throw new LobbyOperationException("player_not_found", exception.Message, (int)HttpStatusCode.NotFound);
            }
        }

        return match;
    }

    private async Task<Match> GetMatchOrThrowAsync(string matchId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(matchId, out var matchGuid))
        {
            throw new LobbyOperationException("validation_failed", "Match id is invalid.", (int)HttpStatusCode.BadRequest);
        }

        var match = await matchRepository.GetByIdAsync(new MatchId(matchGuid), cancellationToken);

        if (match is null)
        {
            throw new LobbyOperationException("match_not_found", "Match was not found.", (int)HttpStatusCode.NotFound);
        }

        return match;
    }

    private static PlayerId ParsePlayerId(string playerId)
    {
        if (!Guid.TryParse(playerId, out var value))
        {
            throw new LobbyOperationException("validation_failed", "Player id is invalid.", (int)HttpStatusCode.BadRequest);
        }

        return new PlayerId(value);
    }
}

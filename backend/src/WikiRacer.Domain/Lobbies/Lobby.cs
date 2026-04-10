using WikiRacer.Domain.Languages;
using WikiRacer.Domain.Matches;
using WikiRacer.Domain.Players;

namespace WikiRacer.Domain.Lobbies;

public sealed class Lobby
{
    private readonly List<LobbyPlayer> _players = [];

    public Lobby(
        LobbyId id,
        PublicLobbyId publicId,
        LobbySettings settings,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        Id = id;
        PublicId = publicId;
        Settings = settings;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        Status = LobbyStatus.Waiting;
        Revision = 1;
    }

    public LobbyId Id { get; }

    public PublicLobbyId PublicId { get; }

    public LobbySettings Settings { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset ExpiresAtUtc { get; }

    public LobbyStatus Status { get; private set; }

    public int Revision { get; private set; }

    public DateTimeOffset? CountdownEndsAtUtc { get; private set; }

    public MatchId? ActiveMatchId { get; private set; }

    public IReadOnlyList<LobbyPlayer> Players => _players;

    public bool IsFull => _players.Count >= Settings.PlayerCap;

    public bool IsJoinable(DateTimeOffset now) => Status == LobbyStatus.Waiting && ExpiresAtUtc > now && !IsFull;

    public LobbyPlayer AddHost(PlayerId playerId, string displayName)
    {
        if (_players.Count > 0)
        {
            throw new InvalidOperationException("Lobby already has a host.");
        }

        return AddPlayerInternal(playerId, displayName, isHost: true);
    }

    public LobbyPlayer AddPlayer(PlayerId playerId, string displayName)
    {
        if (Status != LobbyStatus.Waiting)
        {
            throw new InvalidOperationException("Lobby is not accepting players.");
        }

        if (IsFull)
        {
            throw new InvalidOperationException("Lobby is already full.");
        }

        return AddPlayerInternal(playerId, displayName, isHost: false);
    }

    public LobbyPlayer? FindPlayer(PlayerId playerId)
    {
        return _players.SingleOrDefault(player => player.PlayerId == playerId);
    }

    public void UpdateLanguage(PlayerId requestedByPlayerId, WikipediaLanguage language, WikipediaLanguage uiLanguage)
    {
        EnsureHostCanUpdateSettings(requestedByPlayerId);

        Settings.UpdateLanguage(language, uiLanguage);
        IncrementRevision();
    }

    public void UpdateStartArticle(PlayerId requestedByPlayerId, LobbyArticle article, ArticleSelectionMode selectionMode)
    {
        EnsureHostCanUpdateSettings(requestedByPlayerId);
        Settings.UpdateStartArticle(article, selectionMode);
        IncrementRevision();
    }

    public void UpdateTargetArticle(PlayerId requestedByPlayerId, LobbyArticle article, ArticleSelectionMode selectionMode)
    {
        EnsureHostCanUpdateSettings(requestedByPlayerId);
        Settings.UpdateTargetArticle(article, selectionMode);
        IncrementRevision();
    }

    public void SetPlayerReady(PlayerId requestedByPlayerId, bool isReady)
    {
        var player = FindPlayer(requestedByPlayerId);

        if (player is null)
        {
            throw new InvalidOperationException("Player is not part of the lobby.");
        }

        if (Status != LobbyStatus.Waiting)
        {
            throw new InvalidOperationException("Lobby settings can only be changed before the match starts.");
        }

        player.SetReady(isReady);
        IncrementRevision();
    }

    public void MarkPlayerDisconnected(PlayerId requestedByPlayerId)
    {
        var player = FindPlayer(requestedByPlayerId);

        if (player is null)
        {
            throw new InvalidOperationException("Player is not part of the lobby.");
        }

        player.MarkDisconnected();
        IncrementRevision();
    }

    public void MarkPlayerConnected(PlayerId requestedByPlayerId)
    {
        var player = FindPlayer(requestedByPlayerId);

        if (player is null)
        {
            throw new InvalidOperationException("Player is not part of the lobby.");
        }

        player.MarkConnected();
        IncrementRevision();
    }

    public bool CanStartCountdown()
    {
        return Status == LobbyStatus.Waiting
            && Settings.StartArticle is not null
            && Settings.TargetArticle is not null
            && _players.Count > 0
            && _players.All(player => player.IsConnected && player.IsReady);
    }

    public void SetCountdown(DateTimeOffset? endsAtUtc)
    {
        if (CountdownEndsAtUtc == endsAtUtc)
        {
            return;
        }

        CountdownEndsAtUtc = endsAtUtc;
        IncrementRevision();
    }

    public void MarkExpired()
    {
        Status = LobbyStatus.Expired;
        IncrementRevision();
    }

    public void StartMatch(PlayerId requestedByPlayerId, MatchId matchId)
    {
        EnsureHostCanUpdateSettings(requestedByPlayerId);

        if (Settings.StartArticle is null || Settings.TargetArticle is null)
        {
            throw new InvalidOperationException("Lobby articles must be selected before the match starts.");
        }

        Status = LobbyStatus.InMatch;
        CountdownEndsAtUtc = null;
        ActiveMatchId = matchId;
        IncrementRevision();
    }

    public void TouchRevision()
    {
        IncrementRevision();
    }

    private LobbyPlayer AddPlayerInternal(PlayerId playerId, string displayName, bool isHost)
    {
        var player = new LobbyPlayer(playerId, displayName, isHost);
        _players.Add(player);
        IncrementRevision();
        return player;
    }

    private void IncrementRevision()
    {
        Revision++;
    }

    private void EnsureHostCanUpdateSettings(PlayerId requestedByPlayerId)
    {
        var player = FindPlayer(requestedByPlayerId);

        if (player is null)
        {
            throw new InvalidOperationException("Player is not part of the lobby.");
        }

        if (!player.IsHost)
        {
            throw new InvalidOperationException("Only the host can update the lobby language.");
        }

        if (Status != LobbyStatus.Waiting)
        {
            throw new InvalidOperationException("Lobby settings can only be changed before the match starts.");
        }
    }
}

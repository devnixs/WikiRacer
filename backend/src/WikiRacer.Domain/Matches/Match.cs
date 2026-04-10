using WikiRacer.Domain.Lobbies;
using WikiRacer.Domain.Players;

namespace WikiRacer.Domain.Matches;

public sealed class Match
{
    private readonly List<MatchPlayer> _players;

    public Match(
        MatchId id,
        LobbyId lobbyId,
        string publicLobbyId,
        string language,
        LobbyArticle startArticle,
        LobbyArticle targetArticle,
        IEnumerable<MatchPlayer> players,
        DateTimeOffset startedAtUtc)
    {
        Id = id;
        LobbyId = lobbyId;
        PublicLobbyId = publicLobbyId;
        Language = language;
        StartArticle = startArticle;
        TargetArticle = targetArticle;
        StartedAtUtc = startedAtUtc;
        Status = MatchStatus.InProgress;
        _players = players.ToList();
        TimelineSequence = 1;

        foreach (var player in _players.Where(player => player.Status == MatchPlayerRaceStatus.Active))
        {
            player.ReportProgress(startArticle.Title.Value);
        }
    }

    public MatchId Id { get; }

    public LobbyId LobbyId { get; }

    public string PublicLobbyId { get; }

    public string Language { get; }

    public LobbyArticle StartArticle { get; }

    public LobbyArticle TargetArticle { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public MatchStatus Status { get; private set; }

    public long TimelineSequence { get; private set; }

    public IReadOnlyList<MatchPlayer> Players => _players;

    public MatchPlayer? FindPlayer(PlayerId playerId)
    {
        return _players.SingleOrDefault(player => player.PlayerId == playerId);
    }

    public void ReportProgress(PlayerId playerId, string canonicalArticleTitle, DateTimeOffset reportedAtUtc)
    {
        var player = RequirePlayer(playerId);
        player.ReportProgress(canonicalArticleTitle);

        if (string.Equals(canonicalArticleTitle, TargetArticle.Title.Value, StringComparison.Ordinal)
            && player.Status == MatchPlayerRaceStatus.Active)
        {
            var placement = _players.Count(existing => existing.Status == MatchPlayerRaceStatus.Finished) + 1;
            player.Finish(reportedAtUtc - StartedAtUtc, placement);
        }

        EvaluateCompletion();
        TimelineSequence++;
    }

    public void Abandon(PlayerId playerId)
    {
        var player = RequirePlayer(playerId);
        player.Abandon();
        EvaluateCompletion();
        TimelineSequence++;
    }

    private MatchPlayer RequirePlayer(PlayerId playerId)
    {
        return FindPlayer(playerId) ?? throw new InvalidOperationException("Player is not part of the match.");
    }

    private void EvaluateCompletion()
    {
        if (_players.All(player => player.Status != MatchPlayerRaceStatus.Active))
        {
            Status = MatchStatus.Finished;
        }
    }
}

using WikiRacer.Domain.Players;

namespace WikiRacer.Domain.Matches;

public sealed class MatchPlayer
{
    private readonly List<string> _visitedArticleTitles = [];

    public MatchPlayer(PlayerId playerId, string displayName, bool isConnected)
    {
        PlayerId = playerId;
        DisplayName = displayName;
        IsConnected = isConnected;
        Status = MatchPlayerRaceStatus.Active;
    }

    public PlayerId PlayerId { get; }

    public string DisplayName { get; }

    public MatchPlayerRaceStatus Status { get; private set; }

    public string? CurrentArticleTitle { get; private set; }

    public int HopCount { get; private set; }

    public TimeSpan? FinishTime { get; private set; }

    public int? Placement { get; private set; }

    public bool IsConnected { get; private set; }

    public IReadOnlyList<string> VisitedArticleTitles => _visitedArticleTitles;

    public void BeginAtArticle(string canonicalArticleTitle)
    {
        if (Status != MatchPlayerRaceStatus.Active)
        {
            return;
        }

        CurrentArticleTitle = canonicalArticleTitle;

        if (_visitedArticleTitles.Count == 0)
        {
            _visitedArticleTitles.Add(canonicalArticleTitle);
        }
    }

    public void ReportProgress(string canonicalArticleTitle)
    {
        if (Status != MatchPlayerRaceStatus.Active)
        {
            return;
        }

        if (string.Equals(CurrentArticleTitle, canonicalArticleTitle, StringComparison.Ordinal))
        {
            return;
        }

        CurrentArticleTitle = canonicalArticleTitle;
        _visitedArticleTitles.Add(canonicalArticleTitle);
        HopCount++;
    }

    public void Finish(TimeSpan finishTime, int placement)
    {
        Status = MatchPlayerRaceStatus.Finished;
        FinishTime = finishTime;
        Placement = placement;
    }

    public void Abandon()
    {
        if (Status == MatchPlayerRaceStatus.Finished)
        {
            return;
        }

        Status = MatchPlayerRaceStatus.Abandoned;
    }

    public void MarkDisconnected()
    {
        IsConnected = false;

        if (Status == MatchPlayerRaceStatus.Active)
        {
            Status = MatchPlayerRaceStatus.Disconnected;
        }
    }
}

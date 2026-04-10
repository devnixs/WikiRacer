namespace WikiRacer.Contracts.Matches;

public sealed record MatchPlayerStateView(
    string PlayerId,
    string DisplayName,
    MatchPlayerRaceStatus Status,
    string? CurrentArticleTitle,
    int HopCount,
    TimeSpan? FinishTime,
    int? Placement,
    bool IsConnected);

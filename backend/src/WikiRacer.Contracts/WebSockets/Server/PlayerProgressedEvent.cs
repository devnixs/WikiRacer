namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record PlayerProgressedEvent(
    string MatchId,
    string PlayerId,
    string CurrentArticleTitle,
    int HopCount,
    DateTimeOffset ReportedAtUtc);

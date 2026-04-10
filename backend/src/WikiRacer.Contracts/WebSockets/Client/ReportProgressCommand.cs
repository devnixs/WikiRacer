namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record ReportProgressCommand(
    string MatchId,
    string PlayerId,
    string CurrentArticleTitle,
    int HopCount,
    DateTimeOffset ReportedAtUtc);

namespace WikiRacer.Application.Matches;

public sealed record ReportMatchProgressCommand(
    string MatchId,
    string PlayerId,
    string CurrentArticleTitle,
    DateTimeOffset ReportedAtUtc);

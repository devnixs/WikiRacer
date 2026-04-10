namespace WikiRacer.Contracts.Requests;

public sealed record ReportMatchProgressRequest(
    string PlayerId,
    string CurrentArticleTitle,
    DateTimeOffset ReportedAtUtc);

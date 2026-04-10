namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record PlayerAbandonedEvent(
    string MatchId,
    string PlayerId,
    string? Reason,
    int? Placement,
    DateTimeOffset OccurredAtUtc);

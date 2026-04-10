namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record PlayerFinishedEvent(
    string MatchId,
    string PlayerId,
    int HopCount,
    TimeSpan FinishTime,
    int Placement,
    DateTimeOffset OccurredAtUtc);

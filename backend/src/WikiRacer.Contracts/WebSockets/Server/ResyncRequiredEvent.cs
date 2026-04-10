namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record ResyncRequiredEvent(
    string ScopeType,
    string ScopeId,
    string Reason,
    long LatestSequence);

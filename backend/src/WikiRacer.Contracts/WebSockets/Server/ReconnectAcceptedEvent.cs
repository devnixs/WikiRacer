namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record ReconnectAcceptedEvent(
    string PlayerId,
    string ConnectionToken,
    string ScopeType,
    string ScopeId,
    long NextExpectedSequence);

namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record RequestResyncCommand(
    string ScopeId,
    string ScopeType,
    long? LastReceivedSequence = null);

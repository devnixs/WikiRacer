namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record ReconnectCommand(
    string ReconnectToken,
    string? LobbyId = null,
    string? MatchId = null,
    long? LastReceivedSequence = null);

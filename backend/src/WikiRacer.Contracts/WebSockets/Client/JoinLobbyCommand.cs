namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record JoinLobbyCommand(
    string PublicLobbyId,
    string DisplayName,
    string? ReconnectToken = null);

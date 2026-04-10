namespace WikiRacer.Contracts.Requests;

public sealed record JoinLobbyRequest(
    string DisplayName,
    string? ReconnectToken = null);

namespace WikiRacer.Application.Lobbies;

public sealed record JoinLobbyCommand(
    string PublicLobbyId,
    string? DisplayName,
    string? ReconnectToken);

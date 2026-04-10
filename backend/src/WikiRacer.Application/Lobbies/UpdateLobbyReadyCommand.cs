namespace WikiRacer.Application.Lobbies;

public sealed record UpdateLobbyReadyCommand(
    string PublicLobbyId,
    string PlayerId,
    bool IsReady);

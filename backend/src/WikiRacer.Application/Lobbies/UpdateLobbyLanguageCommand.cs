namespace WikiRacer.Application.Lobbies;

public sealed record UpdateLobbyLanguageCommand(
    string PublicLobbyId,
    string PlayerId,
    string Language);

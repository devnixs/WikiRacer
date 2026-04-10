namespace WikiRacer.Application.Lobbies;

public sealed record CreateLobbyCommand(
    string DisplayName,
    string Language,
    string UiLanguage,
    int PlayerCap,
    int? TimeLimitSeconds);

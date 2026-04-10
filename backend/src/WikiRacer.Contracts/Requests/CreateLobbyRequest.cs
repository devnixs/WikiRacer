namespace WikiRacer.Contracts.Requests;

public sealed record CreateLobbyRequest(
    string DisplayName,
    string Language,
    string UiLanguage,
    int PlayerCap,
    int? TimeLimitSeconds);

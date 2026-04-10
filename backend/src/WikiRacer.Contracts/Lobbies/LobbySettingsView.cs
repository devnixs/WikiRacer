namespace WikiRacer.Contracts.Lobbies;

public sealed record LobbySettingsView(
    string Language,
    string UiLanguage,
    int? TimeLimitSeconds,
    int PlayerCap,
    ArticleReference? StartArticle,
    ArticleReference? TargetArticle,
    ArticleSelectionMode StartSelectionMode,
    ArticleSelectionMode TargetSelectionMode);

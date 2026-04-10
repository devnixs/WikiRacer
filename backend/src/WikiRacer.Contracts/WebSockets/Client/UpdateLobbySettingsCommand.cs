using WikiRacer.Contracts.Lobbies;

namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record UpdateLobbySettingsCommand(
    string LobbyId,
    string RequestedByPlayerId,
    string Language,
    string UiLanguage,
    int? TimeLimitSeconds,
    int PlayerCap,
    ArticleReference? StartArticle,
    ArticleReference? TargetArticle,
    ArticleSelectionMode StartSelectionMode,
    ArticleSelectionMode TargetSelectionMode);

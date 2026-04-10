using WikiRacer.Domain.Common;
using WikiRacer.Domain.Languages;

namespace WikiRacer.Domain.Lobbies;

public sealed class LobbySettings
{
    public const int MinimumPlayerCap = 1;
    public const int MaximumPlayerCap = 32;

    public LobbySettings(
        WikipediaLanguage language,
        WikipediaLanguage uiLanguage,
        int playerCap,
        int? timeLimitSeconds)
    {
        if (playerCap is < MinimumPlayerCap or > MaximumPlayerCap)
        {
            throw new ArgumentOutOfRangeException(nameof(playerCap), $"Player cap must be between {MinimumPlayerCap} and {MaximumPlayerCap}.");
        }

        if (timeLimitSeconds is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeLimitSeconds), "Time limit must be greater than zero when provided.");
        }

        Language = language;
        UiLanguage = uiLanguage;
        PlayerCap = playerCap;
        TimeLimitSeconds = timeLimitSeconds;
        StartSelectionMode = ArticleSelectionMode.Manual;
        TargetSelectionMode = ArticleSelectionMode.Manual;
    }

    public WikipediaLanguage Language { get; private set; }

    public WikipediaLanguage UiLanguage { get; private set; }

    public int PlayerCap { get; private set; }

    public int? TimeLimitSeconds { get; private set; }

    public LobbyArticle? StartArticle { get; private set; }

    public LobbyArticle? TargetArticle { get; private set; }

    public ArticleSelectionMode StartSelectionMode { get; private set; }

    public ArticleSelectionMode TargetSelectionMode { get; private set; }

    public void UpdateLanguage(WikipediaLanguage language, WikipediaLanguage uiLanguage)
    {
        Language = language;
        UiLanguage = uiLanguage;
        StartArticle = null;
        TargetArticle = null;
        StartSelectionMode = ArticleSelectionMode.Manual;
        TargetSelectionMode = ArticleSelectionMode.Manual;
    }

    public void UpdateStartArticle(LobbyArticle article, ArticleSelectionMode selectionMode)
    {
        EnsureDistinctFrom(TargetArticle, article, "start");
        StartArticle = article;
        StartSelectionMode = selectionMode;
    }

    public void UpdateTargetArticle(LobbyArticle article, ArticleSelectionMode selectionMode)
    {
        EnsureDistinctFrom(StartArticle, article, "target");
        TargetArticle = article;
        TargetSelectionMode = selectionMode;
    }

    private static void EnsureDistinctFrom(LobbyArticle? otherArticle, LobbyArticle nextArticle, string slot)
    {
        if (otherArticle is null)
        {
            return;
        }

        if (string.Equals(otherArticle.Title.Value, nextArticle.Title.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Lobby {slot} and {(slot == "start" ? "target" : "start")} articles must be different.");
        }
    }
}

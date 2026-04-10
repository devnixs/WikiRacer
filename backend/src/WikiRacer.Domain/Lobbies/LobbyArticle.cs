using WikiRacer.Domain.Articles;
using WikiRacer.Domain.Common;

namespace WikiRacer.Domain.Lobbies;

public sealed class LobbyArticle
{
    public LobbyArticle(string title, string displayTitle, string canonicalPath)
    {
        Title = new ArticleTitle(title);
        DisplayTitle = Guard.AgainstNullOrWhiteSpace(displayTitle, nameof(displayTitle));
        CanonicalPath = Guard.AgainstNullOrWhiteSpace(canonicalPath, nameof(canonicalPath));
    }

    public ArticleTitle Title { get; }

    public string DisplayTitle { get; }

    public string CanonicalPath { get; }
}

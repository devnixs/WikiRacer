using WikiRacer.Application.Articles;
using WikiRacer.Domain.Languages;

namespace WikiRacer.Application.Abstractions.Articles;

public interface IWikipediaArticleClient
{
    Task<IReadOnlyList<ArticleSearchSuggestion>> SearchAsync(WikipediaLanguage language, string query, CancellationToken cancellationToken);

    Task<ResolvedArticle?> ResolveAsync(WikipediaLanguage language, string title, CancellationToken cancellationToken);

    Task<string?> GetArticleHtmlAsync(WikipediaLanguage language, string canonicalTitle, CancellationToken cancellationToken);
}

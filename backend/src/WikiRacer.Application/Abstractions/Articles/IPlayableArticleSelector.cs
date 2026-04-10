using WikiRacer.Application.Articles;
using WikiRacer.Domain.Languages;

namespace WikiRacer.Application.Abstractions.Articles;

public interface IPlayableArticleSelector
{
    Task<ResolvedArticle?> SelectRandomAsync(
        WikipediaLanguage language,
        IReadOnlyCollection<string> excludedTitles,
        CancellationToken cancellationToken);
}

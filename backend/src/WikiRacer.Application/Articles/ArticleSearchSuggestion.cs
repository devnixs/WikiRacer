namespace WikiRacer.Application.Articles;

public sealed record ArticleSearchSuggestion(
    string Title,
    string DisplayTitle,
    string CanonicalPath,
    string? Description = null);

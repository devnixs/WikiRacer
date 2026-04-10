namespace WikiRacer.Contracts.Articles;

public sealed record ArticleSearchResponse(
    string Title,
    string DisplayTitle,
    string CanonicalPath,
    string? Description = null);

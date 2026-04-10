namespace WikiRacer.Contracts.Articles;

public sealed record ArticleRenderResponse(
    string Language,
    string Title,
    string DisplayTitle,
    string CanonicalPath,
    string SourceUrl,
    string Html);

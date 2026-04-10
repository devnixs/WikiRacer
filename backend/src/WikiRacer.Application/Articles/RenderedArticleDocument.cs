namespace WikiRacer.Application.Articles;

public sealed record RenderedArticleDocument(
    string Language,
    string Title,
    string DisplayTitle,
    string CanonicalPath,
    string SourceUrl,
    string Html);

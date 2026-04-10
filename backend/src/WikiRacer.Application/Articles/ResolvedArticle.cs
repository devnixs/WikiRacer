namespace WikiRacer.Application.Articles;

public sealed record ResolvedArticle(
    string Title,
    string DisplayTitle,
    string CanonicalPath);

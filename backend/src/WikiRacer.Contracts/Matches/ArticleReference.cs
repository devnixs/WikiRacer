namespace WikiRacer.Contracts;

public sealed record ArticleReference(
    string Title,
    string DisplayTitle,
    string CanonicalPath);

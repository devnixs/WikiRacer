namespace WikiRacer.Application.Articles;

public sealed record SearchArticlesQuery(
    string Language,
    string Query);

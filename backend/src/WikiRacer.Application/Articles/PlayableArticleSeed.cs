namespace WikiRacer.Application.Articles;

public sealed record PlayableArticleSeed(
    string Title,
    int Recognizability,
    int Breadth,
    int Accessibility);

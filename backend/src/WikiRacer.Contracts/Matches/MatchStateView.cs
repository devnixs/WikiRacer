namespace WikiRacer.Contracts.Matches;

public sealed record MatchStateView(
    string MatchId,
    string LobbyId,
    string Language,
    ArticleReference StartArticle,
    ArticleReference TargetArticle,
    string Status,
    DateTimeOffset StartedAtUtc,
    IReadOnlyList<MatchPlayerStateView> Players,
    long TimelineSequence);

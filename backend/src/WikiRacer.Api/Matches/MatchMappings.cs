using WikiRacer.Contracts;
using WikiRacer.Contracts.Matches;
using WikiRacer.Domain.Matches;

namespace WikiRacer.Api.Matches;

internal static class MatchMappings
{
    public static MatchStateView ToContract(this Match match)
    {
        return new MatchStateView(
            match.Id.ToString(),
            match.LobbyId.ToString(),
            match.Language,
            new ArticleReference(
                match.StartArticle.Title.Value,
                match.StartArticle.DisplayTitle,
                match.StartArticle.CanonicalPath),
            new ArticleReference(
                match.TargetArticle.Title.Value,
                match.TargetArticle.DisplayTitle,
                match.TargetArticle.CanonicalPath),
            match.Status == MatchStatus.InProgress ? "inProgress" : "finished",
            match.StartedAtUtc,
            match.Players.Select(player => new MatchPlayerStateView(
                player.PlayerId.ToString(),
                player.DisplayName,
                (Contracts.Matches.MatchPlayerRaceStatus)player.Status,
                player.CurrentArticleTitle,
                player.HopCount,
                player.FinishTime,
                player.Placement,
                player.IsConnected,
                player.VisitedArticleTitles.ToArray())).ToArray(),
            match.TimelineSequence);
    }
}

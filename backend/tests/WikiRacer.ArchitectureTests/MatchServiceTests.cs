using WikiRacer.Application.Articles;
using WikiRacer.Application.Lobbies;
using WikiRacer.Application.Matches;
using WikiRacer.Contracts.Errors;
using WikiRacer.Domain.Languages;
using WikiRacer.Infrastructure.Clock;
using WikiRacer.Infrastructure.Lobbies;
using WikiRacer.Infrastructure.Matches;

namespace WikiRacer.ArchitectureTests;

public class MatchServiceTests
{
    [Fact]
    public async Task StartAsync_Should_Transition_Lobby_Into_Match()
    {
        var lobbyRepository = new InMemoryLobbyRepository();
        var matchRepository = new InMemoryMatchRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(lobbyRepository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 4, 600), CancellationToken.None);
        var guest = await lobbyService.JoinAsync(new JoinLobbyCommand(createResult.Lobby.PublicId.Value, "Guest", null), CancellationToken.None);
        await lobbyService.UpdateReadyAsync(new UpdateLobbyReadyCommand(createResult.Lobby.PublicId.Value, guest.Session.PlayerId.ToString(), true), CancellationToken.None);

        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new FakePlayableArticleSelector(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock(),
            lobbyRepository);
        await articleService.UpdateLobbyArticleAsync(new UpdateLobbyArticleCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString(), "start", "Paris"), CancellationToken.None);
        var targetArticleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Lyon", "Lyon", "/wiki/Lyon")),
            new FakePlayableArticleSelector(new ResolvedArticle("Lyon", "Lyon", "/wiki/Lyon")),
            new SystemClock(),
            lobbyRepository);
        await targetArticleService.UpdateLobbyArticleAsync(new UpdateLobbyArticleCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString(), "target", "Lyon"), CancellationToken.None);

        var matchService = new MatchService(
            lobbyRepository,
            matchRepository,
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock());

        var match = await matchService.StartAsync(new StartMatchCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString()), CancellationToken.None);

        Assert.Equal("fr", match.Language);
        Assert.Equal(WikiRacer.Domain.Lobbies.LobbyStatus.InMatch, createResult.Lobby.Status);
        Assert.Equal(2, match.Players.Count);
        Assert.All(match.Players, player =>
        {
            Assert.Equal("Paris", player.CurrentArticleTitle);
            Assert.Equal(0, player.HopCount);
        });
    }

    [Fact]
    public async Task JoinAsync_Should_Add_Player_To_Active_Match()
    {
        var lobbyRepository = new InMemoryLobbyRepository();
        var matchRepository = new InMemoryMatchRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(lobbyRepository, matchRepository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 3, 600), CancellationToken.None);
        await SetupMatchArticlesAsync(lobbyRepository, createResult);

        var matchService = new MatchService(
            lobbyRepository,
            matchRepository,
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock());

        var match = await matchService.StartAsync(new StartMatchCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString()), CancellationToken.None);
        var joinResult = await lobbyService.JoinAsync(new JoinLobbyCommand(createResult.Lobby.PublicId.Value, "Late Guest", null), CancellationToken.None);

        Assert.Equal(WikiRacer.Domain.Lobbies.LobbyStatus.InMatch, joinResult.Lobby.Status);
        Assert.Contains(joinResult.Lobby.Players, player => player.PlayerId == joinResult.Session.PlayerId && player.DisplayName == "Late Guest");
        Assert.Contains(match.Players, player =>
            player.PlayerId == joinResult.Session.PlayerId
            && player.DisplayName == "Late Guest"
            && player.CurrentArticleTitle == "Paris"
            && player.HopCount == 0
            && player.Status == WikiRacer.Domain.Matches.MatchPlayerRaceStatus.Active);
    }

    [Fact]
    public async Task ReportProgressAsync_Should_Finish_Player_When_Target_Is_Reached()
    {
        var lobbyRepository = new InMemoryLobbyRepository();
        var matchRepository = new InMemoryMatchRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(lobbyRepository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 2, 600), CancellationToken.None);
        await SetupMatchArticlesAsync(lobbyRepository, createResult);

        var matchService = new MatchService(
            lobbyRepository,
            matchRepository,
            new FakeWikipediaArticleClient(new ResolvedArticle("Lyon", "Lyon", "/wiki/Lyon")),
            new SystemClock());

        var match = await matchService.StartAsync(new StartMatchCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString()), CancellationToken.None);
        var updated = await matchService.ReportProgressAsync(
            new ReportMatchProgressCommand(match.Id.ToString(), createResult.Session.PlayerId.ToString(), "Lyon", DateTimeOffset.UtcNow),
            CancellationToken.None);

        Assert.Equal(WikiRacer.Domain.Matches.MatchPlayerRaceStatus.Finished, updated.Players[0].Status);
        Assert.Equal(WikiRacer.Domain.Matches.MatchStatus.Finished, updated.Status);
    }

    [Fact]
    public async Task AbandonAsync_Should_Record_Player_Abandonment()
    {
        var lobbyRepository = new InMemoryLobbyRepository();
        var matchRepository = new InMemoryMatchRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(lobbyRepository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 2, 600), CancellationToken.None);
        await SetupMatchArticlesAsync(lobbyRepository, createResult);

        var matchService = new MatchService(
            lobbyRepository,
            matchRepository,
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock());

        var match = await matchService.StartAsync(new StartMatchCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString()), CancellationToken.None);
        var updated = await matchService.AbandonAsync(new AbandonMatchCommand(match.Id.ToString(), createResult.Session.PlayerId.ToString()), CancellationToken.None);

        Assert.Equal(WikiRacer.Domain.Matches.MatchPlayerRaceStatus.Abandoned, updated.Players[0].Status);
        Assert.Equal(WikiRacer.Domain.Matches.MatchStatus.Finished, updated.Status);
    }

    private static async Task SetupMatchArticlesAsync(InMemoryLobbyRepository lobbyRepository, CreateLobbyResult createResult)
    {
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new FakePlayableArticleSelector(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock(),
            lobbyRepository);

        await articleService.UpdateLobbyArticleAsync(new UpdateLobbyArticleCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString(), "start", "Paris"), CancellationToken.None);

        var lyonArticleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Lyon", "Lyon", "/wiki/Lyon")),
            new FakePlayableArticleSelector(new ResolvedArticle("Lyon", "Lyon", "/wiki/Lyon")),
            new SystemClock(),
            lobbyRepository);

        await lyonArticleService.UpdateLobbyArticleAsync(new UpdateLobbyArticleCommand(createResult.Lobby.PublicId.Value, createResult.Session.PlayerId.ToString(), "target", "Lyon"), CancellationToken.None);
    }

    private sealed class FakeWikipediaArticleClient(
        ResolvedArticle resolvedArticle) : WikiRacer.Application.Abstractions.Articles.IWikipediaArticleClient
    {
        public Task<IReadOnlyList<ArticleSearchSuggestion>> SearchAsync(WikipediaLanguage language, string query, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ArticleSearchSuggestion>>([]);

        public Task<ResolvedArticle?> ResolveAsync(WikipediaLanguage language, string title, CancellationToken cancellationToken)
            => Task.FromResult<ResolvedArticle?>(resolvedArticle);

        public Task<string?> GetArticleHtmlAsync(WikipediaLanguage language, string canonicalTitle, CancellationToken cancellationToken)
            => Task.FromResult<string?>(null);
    }

    private sealed class FakePlayableArticleSelector(ResolvedArticle? resolvedArticle) : WikiRacer.Application.Abstractions.Articles.IPlayableArticleSelector
    {
        public Task<ResolvedArticle?> SelectRandomAsync(WikipediaLanguage language, IReadOnlyCollection<string> excludedTitles, CancellationToken cancellationToken)
            => Task.FromResult(resolvedArticle);
    }
}

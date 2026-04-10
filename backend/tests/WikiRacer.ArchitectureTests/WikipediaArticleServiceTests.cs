using System.Net;
using WikiRacer.Application.Abstractions.Articles;
using WikiRacer.Application.Articles;
using WikiRacer.Application.Lobbies;
using WikiRacer.Contracts.Errors;
using WikiRacer.Domain.Languages;
using WikiRacer.Infrastructure.Clock;
using WikiRacer.Infrastructure.Lobbies;

namespace WikiRacer.ArchitectureTests;

public class WikipediaArticleServiceTests
{
    [Fact]
    public async Task UpdateLobbyArticle_Should_Set_Start_Article_On_Lobby()
    {
        var repository = new InMemoryLobbyRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(repository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 4, 600), CancellationToken.None);
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(
                new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new FakePlayableArticleSelector(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock(),
            repository);

        var updated = await articleService.UpdateLobbyArticleAsync(
            new UpdateLobbyArticleCommand(
                createResult.Lobby.PublicId.Value,
                createResult.Session.PlayerId.ToString(),
                "start",
                "Paris"),
            CancellationToken.None);

        Assert.Equal("Paris", updated.Settings.StartArticle?.Title.Value);
        Assert.Null(updated.Settings.TargetArticle);
    }

    [Fact]
    public async Task UpdateLobbyArticle_Should_Reject_NonHost()
    {
        var repository = new InMemoryLobbyRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(repository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 4, 600), CancellationToken.None);
        var joinResult = await lobbyService.JoinAsync(new JoinLobbyCommand(createResult.Lobby.PublicId.Value, "Guest", null), CancellationToken.None);
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new FakePlayableArticleSelector(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock(),
            repository);

        var exception = await Assert.ThrowsAsync<LobbyOperationException>(() =>
            articleService.UpdateLobbyArticleAsync(
                new UpdateLobbyArticleCommand(
                    createResult.Lobby.PublicId.Value,
                    joinResult.Session.PlayerId.ToString(),
                    "target",
                    "Paris"),
                CancellationToken.None));

        Assert.Equal(ErrorCodes.UnauthorizedAction, exception.ErrorCode);
        Assert.Equal((int)HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task Search_Should_Return_LanguageScoped_Suggestions()
    {
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(
                new ResolvedArticle("Paris", "Paris", "/wiki/Paris"),
                [new ArticleSearchSuggestion("Paris", "Paris", "/wiki/Paris")]),
            new FakePlayableArticleSelector(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock(),
            new InMemoryLobbyRepository());

        var results = await articleService.SearchAsync(new SearchArticlesQuery("fr", "Par"), CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Paris", results[0].Title);
    }

    [Fact]
    public async Task Render_Should_Return_Canonical_Article_Document()
    {
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(
                new ResolvedArticle("Paris", "Paris", "/wiki/Paris"),
                html: "<html><body><p>Paris body</p></body></html>"),
            new FakePlayableArticleSelector(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new SystemClock(),
            new InMemoryLobbyRepository());

        var result = await articleService.RenderAsync(new RenderArticleQuery("fr", "Paris"), CancellationToken.None);

        Assert.Equal("Paris", result.Title);
        Assert.Equal("https://fr.wikipedia.org/wiki/Paris", result.SourceUrl);
        Assert.Contains("Paris body", result.Html);
    }

    [Fact]
    public async Task RandomizeLobbyArticle_Should_Set_Random_Mode_And_Exclude_Other_Selected_Title()
    {
        var repository = new InMemoryLobbyRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(repository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 4, 600), CancellationToken.None);
        var selector = new FakePlayableArticleSelector(new ResolvedArticle("Tour Eiffel", "Tour Eiffel", "/wiki/Tour_Eiffel"));
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            selector,
            new SystemClock(),
            repository);

        await articleService.UpdateLobbyArticleAsync(
            new UpdateLobbyArticleCommand(
                createResult.Lobby.PublicId.Value,
                createResult.Session.PlayerId.ToString(),
                "start",
                "Paris"),
            CancellationToken.None);

        var updated = await articleService.RandomizeLobbyArticleAsync(
            new RandomizeLobbyArticleCommand(
                createResult.Lobby.PublicId.Value,
                createResult.Session.PlayerId.ToString(),
                "target"),
            CancellationToken.None);

        Assert.Equal("Tour Eiffel", updated.Settings.TargetArticle?.Title.Value);
        Assert.Equal(Domain.Lobbies.ArticleSelectionMode.Random, updated.Settings.TargetSelectionMode);
        Assert.Contains("Paris", selector.LastExcludedTitles);
    }

    [Fact]
    public async Task UpdateLobbyArticle_Should_Reject_When_Start_And_Target_Are_The_Same()
    {
        var repository = new InMemoryLobbyRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(repository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 4, 600), CancellationToken.None);
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            new FakePlayableArticleSelector(new ResolvedArticle("Tour Eiffel", "Tour Eiffel", "/wiki/Tour_Eiffel")),
            new SystemClock(),
            repository);

        await articleService.UpdateLobbyArticleAsync(
            new UpdateLobbyArticleCommand(
                createResult.Lobby.PublicId.Value,
                createResult.Session.PlayerId.ToString(),
                "start",
                "Paris"),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<LobbyOperationException>(() =>
            articleService.UpdateLobbyArticleAsync(
                new UpdateLobbyArticleCommand(
                    createResult.Lobby.PublicId.Value,
                    createResult.Session.PlayerId.ToString(),
                    "target",
                    "Paris"),
                CancellationToken.None));

        Assert.Equal(ErrorCodes.LobbyNotJoinable, exception.ErrorCode);
        Assert.Equal((int)HttpStatusCode.Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task RandomizeLobbyArticle_Should_Retry_When_A_Concurrent_Selection_Makes_The_First_Result_Invalid()
    {
        var repository = new InMemoryLobbyRepository();
        var lobbyService = LobbyServiceTestsFactory.Create(repository);
        var createResult = await lobbyService.CreateAsync(new CreateLobbyCommand("Host", "fr", "fr", 4, 600), CancellationToken.None);
        var selector = new FakePlayableArticleSelector(
            new ResolvedArticle("Paris", "Paris", "/wiki/Paris"),
            new ResolvedArticle("Tour Eiffel", "Tour Eiffel", "/wiki/Tour_Eiffel"));
        var articleService = new WikipediaArticleService(
            new FakeWikipediaArticleClient(new ResolvedArticle("Paris", "Paris", "/wiki/Paris")),
            selector,
            new SystemClock(),
            repository);

        await articleService.UpdateLobbyArticleAsync(
            new UpdateLobbyArticleCommand(
                createResult.Lobby.PublicId.Value,
                createResult.Session.PlayerId.ToString(),
                "start",
                "Paris"),
            CancellationToken.None);

        var updated = await articleService.RandomizeLobbyArticleAsync(
            new RandomizeLobbyArticleCommand(
                createResult.Lobby.PublicId.Value,
                createResult.Session.PlayerId.ToString(),
                "target"),
            CancellationToken.None);

        Assert.Equal("Tour Eiffel", updated.Settings.TargetArticle?.Title.Value);
        Assert.Equal(2, selector.CallCount);
    }

    private sealed class FakeWikipediaArticleClient(
        ResolvedArticle resolvedArticle,
        IReadOnlyList<ArticleSearchSuggestion>? suggestions = null,
        string? html = null) : IWikipediaArticleClient
    {
        public Task<IReadOnlyList<ArticleSearchSuggestion>> SearchAsync(WikipediaLanguage language, string query, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ArticleSearchSuggestion>>(suggestions ?? []);
        }

        public Task<ResolvedArticle?> ResolveAsync(WikipediaLanguage language, string title, CancellationToken cancellationToken)
        {
            return Task.FromResult<ResolvedArticle?>(resolvedArticle);
        }

        public Task<string?> GetArticleHtmlAsync(WikipediaLanguage language, string canonicalTitle, CancellationToken cancellationToken)
        {
            return Task.FromResult(html);
        }
    }

    private sealed class FakePlayableArticleSelector(params ResolvedArticle?[] resolvedArticles) : IPlayableArticleSelector
    {
        private readonly Queue<ResolvedArticle?> _resolvedArticles = new(resolvedArticles);

        public IReadOnlyCollection<string> LastExcludedTitles { get; private set; } = [];

        public int CallCount { get; private set; }

        public Task<ResolvedArticle?> SelectRandomAsync(
            WikipediaLanguage language,
            IReadOnlyCollection<string> excludedTitles,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastExcludedTitles = excludedTitles.ToArray();
            return Task.FromResult(_resolvedArticles.Count > 0 ? _resolvedArticles.Dequeue() : null);
        }
    }
}

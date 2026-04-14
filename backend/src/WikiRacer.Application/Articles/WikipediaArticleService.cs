using System.Net;
using WikiRacer.Application.Abstractions.Clock;
using WikiRacer.Application.Abstractions.Articles;
using WikiRacer.Application.Abstractions.Lobbies;
using WikiRacer.Application.Lobbies;
using WikiRacer.Domain.Languages;
using WikiRacer.Domain.Lobbies;
using WikiRacer.Domain.Players;

namespace WikiRacer.Application.Articles;

public sealed class WikipediaArticleService(
    IWikipediaArticleClient wikipediaArticleClient,
    IPlayableArticleSelector playableArticleSelector,
    IClock clock,
    ILobbyRepository lobbyRepository)
{
    public async Task<IReadOnlyList<ArticleSearchSuggestion>> SearchAsync(SearchArticlesQuery query, CancellationToken cancellationToken)
    {
        var language = new WikipediaLanguage(query.Language);
        return await wikipediaArticleClient.SearchAsync(language, query.Query, cancellationToken);
    }

    public async Task<RenderedArticleDocument> RenderAsync(RenderArticleQuery query, CancellationToken cancellationToken)
    {
        var language = new WikipediaLanguage(query.Language);
        var resolved = await wikipediaArticleClient.ResolveAsync(language, query.Title, cancellationToken);

        if (resolved is null)
        {
            throw new LobbyOperationException("article_not_found", "Article was not found.", (int)HttpStatusCode.NotFound);
        }

        var html = await wikipediaArticleClient.GetArticleHtmlAsync(language, resolved.Title, cancellationToken);

        if (string.IsNullOrWhiteSpace(html))
        {
            throw new LobbyOperationException("article_not_found", "Article content was not found.", (int)HttpStatusCode.NotFound);
        }

        return new RenderedArticleDocument(
            language.Value,
            resolved.Title,
            resolved.DisplayTitle,
            resolved.CanonicalPath,
            ToSourceUrl(language.Value, resolved.CanonicalPath),
            html);
    }

    public async Task<Lobby> UpdateLobbyArticleAsync(UpdateLobbyArticleCommand command, CancellationToken cancellationToken)
    {
        var lobby = await lobbyRepository.GetByPublicIdAsync(new PublicLobbyId(command.PublicLobbyId), cancellationToken);

        if (lobby is null)
        {
            throw new LobbyOperationException("lobby_not_found", "Lobby was not found.", (int)HttpStatusCode.NotFound);
        }

        var playerId = ParsePlayerId(command.PlayerId);
        var resolved = await wikipediaArticleClient.ResolveAsync(lobby.Settings.Language, command.Title, cancellationToken);

        if (resolved is null)
        {
            throw new LobbyOperationException("validation_failed", "Article could not be resolved to a playable Wikipedia page.", (int)HttpStatusCode.BadRequest);
        }

        var article = new LobbyArticle(resolved.Title, resolved.DisplayTitle, resolved.CanonicalPath);

        lock (lobby)
        {
            try
            {
                if (string.Equals(command.Slot, "start", StringComparison.OrdinalIgnoreCase))
                {
                    lobby.UpdateStartArticle(playerId, article, Domain.Lobbies.ArticleSelectionMode.Manual);
                    ApplyCountdownState(lobby, clock.UtcNow);
                }
                else if (string.Equals(command.Slot, "target", StringComparison.OrdinalIgnoreCase))
                {
                    lobby.UpdateTargetArticle(playerId, article, Domain.Lobbies.ArticleSelectionMode.Manual);
                    ApplyCountdownState(lobby, clock.UtcNow);
                }
                else
                {
                    throw new LobbyOperationException("validation_failed", "Article slot is invalid.", (int)HttpStatusCode.BadRequest);
                }
            }
            catch (InvalidOperationException exception) when (exception.Message.Contains("host", StringComparison.OrdinalIgnoreCase))
            {
                throw new LobbyOperationException("unauthorized_action", exception.Message, (int)HttpStatusCode.Forbidden);
            }
            catch (InvalidOperationException exception) when (exception.Message.Contains("part of the lobby", StringComparison.OrdinalIgnoreCase))
            {
                throw new LobbyOperationException("player_not_found", exception.Message, (int)HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException exception)
            {
                throw new LobbyOperationException("lobby_not_joinable", exception.Message, (int)HttpStatusCode.Conflict);
            }
        }

        return lobby;
    }

    public async Task<Lobby> RandomizeLobbyArticleAsync(RandomizeLobbyArticleCommand command, CancellationToken cancellationToken)
    {
        var lobby = await lobbyRepository.GetByPublicIdAsync(new PublicLobbyId(command.PublicLobbyId), cancellationToken);

        if (lobby is null)
        {
            throw new LobbyOperationException("lobby_not_found", "Lobby was not found.", (int)HttpStatusCode.NotFound);
        }

        var playerId = ParsePlayerId(command.PlayerId);
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var excludedTitles = GetExcludedTitles(lobby, command.Slot);
            var resolved = await playableArticleSelector.SelectRandomAsync(lobby.Settings.Language, excludedTitles, cancellationToken);

            if (resolved is null)
            {
                throw new LobbyOperationException("validation_failed", "No playable article is available for random selection.", (int)HttpStatusCode.BadRequest);
            }

            var article = new LobbyArticle(resolved.Title, resolved.DisplayTitle, resolved.CanonicalPath);

            lock (lobby)
            {
                try
                {
                    if (WouldDuplicateOppositeArticle(lobby, command.Slot, article))
                    {
                        continue;
                    }

                    if (string.Equals(command.Slot, "start", StringComparison.OrdinalIgnoreCase))
                    {
                        lobby.UpdateStartArticle(playerId, article, Domain.Lobbies.ArticleSelectionMode.Random);
                        ApplyCountdownState(lobby, clock.UtcNow);
                    }
                    else if (string.Equals(command.Slot, "target", StringComparison.OrdinalIgnoreCase))
                    {
                        lobby.UpdateTargetArticle(playerId, article, Domain.Lobbies.ArticleSelectionMode.Random);
                        ApplyCountdownState(lobby, clock.UtcNow);
                    }
                    else
                    {
                        throw new LobbyOperationException("validation_failed", "Article slot is invalid.", (int)HttpStatusCode.BadRequest);
                    }
                }
                catch (InvalidOperationException exception) when (exception.Message.Contains("host", StringComparison.OrdinalIgnoreCase))
                {
                    throw new LobbyOperationException("unauthorized_action", exception.Message, (int)HttpStatusCode.Forbidden);
                }
                catch (InvalidOperationException exception) when (exception.Message.Contains("part of the lobby", StringComparison.OrdinalIgnoreCase))
                {
                    throw new LobbyOperationException("player_not_found", exception.Message, (int)HttpStatusCode.NotFound);
                }
                catch (InvalidOperationException exception)
                {
                    throw new LobbyOperationException("lobby_not_joinable", exception.Message, (int)HttpStatusCode.Conflict);
                }
            }

            return lobby;
        }

        throw new LobbyOperationException("validation_failed", "No distinct playable article is available for random selection.", (int)HttpStatusCode.BadRequest);
    }

    private static PlayerId ParsePlayerId(string playerId)
    {
        if (!Guid.TryParse(playerId, out var value))
        {
            throw new LobbyOperationException("validation_failed", "Player id is invalid.", (int)HttpStatusCode.BadRequest);
        }

        return new PlayerId(value);
    }

    private static IReadOnlyCollection<string> GetExcludedTitles(Lobby lobby, string slot)
    {
        if (string.Equals(slot, "start", StringComparison.OrdinalIgnoreCase))
        {
            return lobby.Settings.TargetArticle is null ? [] : [lobby.Settings.TargetArticle.Title.Value];
        }

        if (string.Equals(slot, "target", StringComparison.OrdinalIgnoreCase))
        {
            return lobby.Settings.StartArticle is null ? [] : [lobby.Settings.StartArticle.Title.Value];
        }

        throw new LobbyOperationException("validation_failed", "Article slot is invalid.", (int)HttpStatusCode.BadRequest);
    }

    private static bool WouldDuplicateOppositeArticle(Lobby lobby, string slot, LobbyArticle article)
    {
        var opposite = string.Equals(slot, "start", StringComparison.OrdinalIgnoreCase)
            ? lobby.Settings.TargetArticle
            : string.Equals(slot, "target", StringComparison.OrdinalIgnoreCase)
                ? lobby.Settings.StartArticle
                : throw new LobbyOperationException("validation_failed", "Article slot is invalid.", (int)HttpStatusCode.BadRequest);

        return opposite is not null
            && string.Equals(opposite.Title.Value, article.Title.Value, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToSourceUrl(string language, string canonicalPath)
    {
        if (Uri.TryCreate(canonicalPath, UriKind.Absolute, out var absoluteUri)
            && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return absoluteUri.ToString();
        }

        return $"https://{language}.wikipedia.org{canonicalPath}";
    }

    private static void ApplyCountdownState(Lobby lobby, DateTimeOffset now)
    {
        if (lobby.CountdownEndsAtUtc is not null && lobby.CountdownEndsAtUtc <= now)
        {
            lobby.SetCountdown(null);
        }

        lobby.SetCountdown(null);
    }
}

using Microsoft.AspNetCore.Mvc;
using WikiRacer.Api.Lobbies;
using WikiRacer.Application.Articles;
using WikiRacer.Application.Lobbies;
using WikiRacer.Contracts.Articles;
using WikiRacer.Contracts.Requests;
using WikiRacer.Infrastructure.Articles;

namespace WikiRacer.Api.Controllers;

[Route("api")]
public sealed class ArticlesController(WikipediaArticleService articleService, LobbyRealtimeHub realtimeHub) : ApiControllerBase
{
    [HttpGet("articles/search")]
    public async Task<ActionResult<IEnumerable<ArticleSearchResponse>>> Search(
        [FromQuery] string language,
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await articleService.SearchAsync(new SearchArticlesQuery(language, query), cancellationToken);

            return Ok(results.Select(result => new ArticleSearchResponse(
                result.Title,
                result.DisplayTitle,
                result.CanonicalPath,
                result.Description)));
        }
        catch (ArgumentException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpGet("articles/render")]
    public async Task<ActionResult<ArticleRenderResponse>> Render(
        [FromQuery] string language,
        [FromQuery] string title,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await articleService.RenderAsync(new RenderArticleQuery(language, title), cancellationToken);

            return Ok(new ArticleRenderResponse(
                document.Language,
                document.Title,
                document.DisplayTitle,
                document.CanonicalPath,
                document.SourceUrl,
                document.Html));
        }
        catch (ArgumentException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpPut("lobbies/{publicLobbyId}/articles/{slot}")]
    public async Task<ActionResult> UpdateLobbyArticle(
        string publicLobbyId,
        string slot,
        [FromBody] UpdateLobbyArticleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var lobby = await articleService.UpdateLobbyArticleAsync(
                new UpdateLobbyArticleCommand(publicLobbyId, request.PlayerId, slot, request.Title),
                cancellationToken);

            await realtimeHub.BroadcastLobbyUpdatedAsync(lobby, $"article_{slot}_changed", cancellationToken);

            return Ok(lobby.ToContract());
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
        catch (ArgumentException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpPost("lobbies/{publicLobbyId}/articles/{slot}/randomize")]
    public async Task<ActionResult> RandomizeLobbyArticle(
        string publicLobbyId,
        string slot,
        [FromBody] RandomizeLobbyArticleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var lobby = await articleService.RandomizeLobbyArticleAsync(
                new RandomizeLobbyArticleCommand(publicLobbyId, request.PlayerId, slot),
                cancellationToken);

            await realtimeHub.BroadcastLobbyUpdatedAsync(lobby, $"article_{slot}_randomized", cancellationToken);

            return Ok(lobby.ToContract());
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
        catch (ArgumentException exception)
        {
            return ValidationError(exception);
        }
    }
}

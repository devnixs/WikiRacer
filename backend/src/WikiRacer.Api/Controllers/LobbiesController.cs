using Microsoft.AspNetCore.Mvc;
using WikiRacer.Api.Lobbies;
using WikiRacer.Api.Matches;
using WikiRacer.Application.Lobbies;
using WikiRacer.Application.Matches;
using WikiRacer.Application.Abstractions.Lobbies;
using WikiRacer.Contracts.Requests;
using WikiRacer.Infrastructure.Lobbies;

namespace WikiRacer.Api.Controllers;

[Route("api/lobbies")]
public sealed class LobbiesController(
    ILobbyService lobbyService,
    MatchService matchService,
    LobbyRealtimeHub realtimeHub) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateLobbyResponse>> Create(
        [FromBody] CreateLobbyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await lobbyService.CreateAsync(
                new CreateLobbyCommand(
                    request.DisplayName,
                    request.Language,
                    request.UiLanguage,
                    request.PlayerCap,
                    request.TimeLimitSeconds),
                cancellationToken);

            var lobbyUrl = $"{Request.Scheme}://{Request.Host}/lobby/{result.Lobby.PublicId.Value}";

            return Created(
                $"/api/lobbies/{result.Lobby.PublicId.Value}",
                new CreateLobbyResponse(
                    lobbyUrl,
                    result.Session.PlayerId.ToString(),
                    result.Session.ReconnectToken,
                    result.Lobby.ToContract()));
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }

    [HttpGet("{publicLobbyId}")]
    public async Task<ActionResult> GetByPublicId(string publicLobbyId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await lobbyService.GetByPublicIdAsync(publicLobbyId, cancellationToken);
            return Ok(result.Lobby.ToContract());
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }

    [HttpPost("{publicLobbyId}/join")]
    public async Task<ActionResult<JoinLobbyResponse>> Join(
        string publicLobbyId,
        [FromBody] JoinLobbyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await lobbyService.JoinAsync(
                new JoinLobbyCommand(publicLobbyId, request.DisplayName, request.ReconnectToken),
                cancellationToken);

            await realtimeHub.BroadcastLobbyUpdatedAsync(
                result.Lobby,
                result.WasReconnect ? "player_reconnected" : "player_joined",
                cancellationToken);

            return Ok(new JoinLobbyResponse(
                result.Session.PlayerId.ToString(),
                result.Session.ConnectionToken,
                result.Session.ReconnectToken,
                result.Lobby.ToContract()));
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }

    [HttpPut("{publicLobbyId}/language")]
    public async Task<ActionResult> UpdateLanguage(
        string publicLobbyId,
        [FromBody] UpdateLobbyLanguageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await lobbyService.UpdateLanguageAsync(
                new UpdateLobbyLanguageCommand(publicLobbyId, request.PlayerId, request.Language),
                cancellationToken);

            await realtimeHub.BroadcastLobbyUpdatedAsync(result.Lobby, "language_changed", cancellationToken);

            return Ok(result.Lobby.ToContract());
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }

    [HttpPut("{publicLobbyId}/players/{playerId}/ready")]
    public async Task<ActionResult> UpdateReady(
        string publicLobbyId,
        string playerId,
        [FromBody] UpdateLobbyReadyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await lobbyService.UpdateReadyAsync(
                new UpdateLobbyReadyCommand(publicLobbyId, playerId, request.IsReady),
                cancellationToken);

            await realtimeHub.BroadcastLobbyUpdatedAsync(result.Lobby, "player_ready_changed", cancellationToken);

            return Ok(result.Lobby.ToContract());
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }

    [HttpPost("{publicLobbyId}/match/start")]
    public async Task<ActionResult> StartMatch(
        string publicLobbyId,
        [FromBody] StartMatchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var match = await matchService.StartAsync(
                new StartMatchCommand(publicLobbyId, request.RequestedByPlayerId),
                cancellationToken);

            var lobby = (await lobbyService.GetByPublicIdAsync(publicLobbyId, cancellationToken)).Lobby;
            await realtimeHub.BroadcastLobbyUpdatedAsync(lobby, "match_started", cancellationToken);

            return Ok(match.ToContract());
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }

    [HttpGet("{publicLobbyId}/match")]
    public async Task<ActionResult> GetMatch(string publicLobbyId, CancellationToken cancellationToken)
    {
        try
        {
            var match = await matchService.GetByLobbyPublicIdAsync(publicLobbyId, cancellationToken);
            return Ok(match.ToContract());
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }
}

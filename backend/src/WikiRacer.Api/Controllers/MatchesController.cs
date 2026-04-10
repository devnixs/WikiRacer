using Microsoft.AspNetCore.Mvc;
using WikiRacer.Api.Lobbies;
using WikiRacer.Api.Matches;
using WikiRacer.Application.Lobbies;
using WikiRacer.Application.Matches;
using WikiRacer.Contracts.Requests;
using WikiRacer.Domain.Matches;

namespace WikiRacer.Api.Controllers;

[Route("api/matches")]
public sealed class MatchesController(
    MatchService matchService,
    LobbyRealtimeHub realtimeHub) : ApiControllerBase
{
    [HttpPost("{matchId}/progress")]
    public async Task<ActionResult> ReportProgress(
        string matchId,
        [FromBody] ReportMatchProgressRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var match = await matchService.ReportProgressAsync(
                new ReportMatchProgressCommand(matchId, request.PlayerId, request.CurrentArticleTitle, request.ReportedAtUtc),
                cancellationToken);

            var matchContract = match.ToContract();
            await realtimeHub.BroadcastMatchSnapshotAsync(match.PublicLobbyId, matchContract, cancellationToken);

            if (Guid.TryParse(request.PlayerId, out var playerGuid))
            {
                var player = match.Players.SingleOrDefault(p => p.PlayerId.Value == playerGuid);

                if (player is { Status: MatchPlayerRaceStatus.Finished, FinishTime: not null, Placement: not null })
                {
                    await realtimeHub.BroadcastPlayerFinishedAsync(
                        match.PublicLobbyId,
                        matchId,
                        request.PlayerId,
                        player.HopCount,
                        player.FinishTime.Value,
                        player.Placement.Value,
                        cancellationToken);
                }
            }

            return Ok(matchContract);
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }

    [HttpPost("{matchId}/abandon")]
    public async Task<ActionResult> Abandon(
        string matchId,
        [FromBody] AbandonMatchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var match = await matchService.AbandonAsync(new AbandonMatchCommand(matchId, request.PlayerId), cancellationToken);
            var matchContract = match.ToContract();

            await realtimeHub.BroadcastMatchSnapshotAsync(match.PublicLobbyId, matchContract, cancellationToken);
            await realtimeHub.BroadcastPlayerAbandonedAsync(match.PublicLobbyId, matchId, request.PlayerId, cancellationToken);

            return Ok(matchContract);
        }
        catch (LobbyOperationException exception)
        {
            return LobbyError(exception);
        }
    }
}

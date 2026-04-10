using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using WikiRacer.Application.Abstractions.Lobbies;
using WikiRacer.Application.Abstractions.Sessions;
using WikiRacer.Application.Lobbies;

namespace WikiRacer.Api.Lobbies;

public static class LobbyRealtimeEndpoints
{
    public static IEndpointRouteBuilder MapLobbyRealtimeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/lobbies/{publicLobbyId}", async (
            HttpContext httpContext,
            string publicLobbyId,
            ILobbyService lobbyService,
            IPlayerSessionStore sessionStore,
            ILobbyRepository lobbyRepository,
            LobbyRealtimeHub realtimeHub,
            CancellationToken cancellationToken) =>
        {
            if (!httpContext.WebSockets.IsWebSocketRequest)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            var reconnectToken = httpContext.Request.Query["reconnectToken"].ToString();

            if (string.IsNullOrWhiteSpace(reconnectToken))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var session = await sessionStore.GetByReconnectTokenAsync(reconnectToken, cancellationToken);

            if (session is null || !string.Equals(session.PublicLobbyId.Value, publicLobbyId, StringComparison.Ordinal))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            WikiRacer.Domain.Lobbies.Lobby lobby;

            try
            {
                lobby = (await lobbyService.GetByPublicIdAsync(publicLobbyId, cancellationToken)).Lobby;
            }
            catch (LobbyOperationException exception)
            {
                httpContext.Response.StatusCode = exception.StatusCode;
                return;
            }

            using var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
            var connectionId = await realtimeHub.RegisterAsync(publicLobbyId, socket, lobby, cancellationToken);

            var buffer = new byte[1024];

            try
            {
                while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(buffer, cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            finally
            {
                await realtimeHub.RemoveAsync(publicLobbyId, connectionId);

                var liveLobby = await lobbyRepository.GetByPublicIdAsync(
                    new WikiRacer.Domain.Lobbies.PublicLobbyId(publicLobbyId),
                    cancellationToken);

                if (liveLobby is not null)
                {
                    lock (liveLobby)
                    {
                        try
                        {
                            liveLobby.MarkPlayerDisconnected(session.PlayerId);
                            liveLobby.SetCountdown(null);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }

                    await realtimeHub.BroadcastLobbyUpdatedAsync(liveLobby, "player_disconnected", cancellationToken);
                }
            }
        });

        return endpoints;
    }
}

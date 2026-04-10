using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WikiRacer.Contracts.Json;
using WikiRacer.Contracts.Matches;
using WikiRacer.Contracts.Protocol;
using WikiRacer.Contracts.WebSockets.Server;
using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Api.Lobbies;

public sealed class LobbyRealtimeHub
{
    private readonly ConcurrentDictionary<string, LobbyScope> _scopes = new(StringComparer.Ordinal);

    public async Task<Guid> RegisterAsync(string publicLobbyId, WebSocket socket, Lobby lobby, CancellationToken cancellationToken)
    {
        var scope = _scopes.GetOrAdd(publicLobbyId, _ => new LobbyScope());
        var connectionId = Guid.NewGuid();
        scope.Connections[connectionId] = socket;

        await SendSnapshotAsync(scope, socket, lobby, isResync: false, cancellationToken);
        return connectionId;
    }

    public Task RemoveAsync(string publicLobbyId, Guid connectionId)
    {
        if (_scopes.TryGetValue(publicLobbyId, out var scope))
        {
            scope.Connections.TryRemove(connectionId, out _);
        }

        return Task.CompletedTask;
    }

    public async Task BroadcastLobbyUpdatedAsync(Lobby lobby, string reason, CancellationToken cancellationToken)
    {
        if (!_scopes.TryGetValue(lobby.PublicId.Value, out var scope))
        {
            return;
        }

        var envelope = new ServerMessageEnvelope<LobbyUpdatedEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.LobbyUpdated,
            Interlocked.Increment(ref scope.Sequence),
            DateTimeOffset.UtcNow,
            null,
            new LobbyUpdatedEvent(
                lobby.Id.ToString(),
                lobby.Revision,
                reason,
                lobby.ToContract()));

        await BroadcastAsync(scope, envelope, cancellationToken);
    }

    public async Task BroadcastMatchSnapshotAsync(string publicLobbyId, MatchStateView match, CancellationToken cancellationToken)
    {
        if (!_scopes.TryGetValue(publicLobbyId, out var scope))
        {
            return;
        }

        var envelope = new ServerMessageEnvelope<MatchSnapshotEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.MatchSnapshot,
            Interlocked.Increment(ref scope.Sequence),
            DateTimeOffset.UtcNow,
            null,
            new MatchSnapshotEvent(match, IsResync: false));

        await BroadcastAsync(scope, envelope, cancellationToken);
    }

    public async Task BroadcastPlayerFinishedAsync(
        string publicLobbyId,
        string matchId,
        string playerId,
        int hopCount,
        TimeSpan finishTime,
        int placement,
        CancellationToken cancellationToken)
    {
        if (!_scopes.TryGetValue(publicLobbyId, out var scope))
        {
            return;
        }

        var envelope = new ServerMessageEnvelope<PlayerFinishedEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.PlayerFinished,
            Interlocked.Increment(ref scope.Sequence),
            DateTimeOffset.UtcNow,
            null,
            new PlayerFinishedEvent(matchId, playerId, hopCount, finishTime, placement, DateTimeOffset.UtcNow));

        await BroadcastAsync(scope, envelope, cancellationToken);
    }

    public async Task BroadcastPlayerAbandonedAsync(
        string publicLobbyId,
        string matchId,
        string playerId,
        CancellationToken cancellationToken)
    {
        if (!_scopes.TryGetValue(publicLobbyId, out var scope))
        {
            return;
        }

        var envelope = new ServerMessageEnvelope<PlayerAbandonedEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.PlayerAbandoned,
            Interlocked.Increment(ref scope.Sequence),
            DateTimeOffset.UtcNow,
            null,
            new PlayerAbandonedEvent(matchId, playerId, Reason: null, Placement: null, DateTimeOffset.UtcNow));

        await BroadcastAsync(scope, envelope, cancellationToken);
    }

    private async Task SendSnapshotAsync(LobbyScope scope, WebSocket socket, Lobby lobby, bool isResync, CancellationToken cancellationToken)
    {
        var envelope = new ServerMessageEnvelope<LobbySnapshotEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.LobbySnapshot,
            Interlocked.Increment(ref scope.Sequence),
            DateTimeOffset.UtcNow,
            null,
            new LobbySnapshotEvent(lobby.ToContract(), isResync));

        await SendAsync(socket, envelope, cancellationToken);
    }

    private async Task BroadcastAsync<TPayload>(LobbyScope scope, ServerMessageEnvelope<TPayload> envelope, CancellationToken cancellationToken)
    {
        foreach (var pair in scope.Connections.ToArray())
        {
            if (pair.Value.State != WebSocketState.Open)
            {
                scope.Connections.TryRemove(pair.Key, out _);
                continue;
            }

            try
            {
                await SendAsync(pair.Value, envelope, cancellationToken);
            }
            catch
            {
                scope.Connections.TryRemove(pair.Key, out _);
            }
        }
    }

    private static async Task SendAsync<TPayload>(WebSocket socket, ServerMessageEnvelope<TPayload> envelope, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(envelope, ContractsJson.Default);
        var bytes = Encoding.UTF8.GetBytes(json);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
    }

    private sealed class LobbyScope
    {
        public ConcurrentDictionary<Guid, WebSocket> Connections { get; } = new();

        public long Sequence;
    }
}

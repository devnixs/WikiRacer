using System.Collections.Concurrent;
using WikiRacer.Application.Abstractions.Sessions;
using WikiRacer.Application.Lobbies;

namespace WikiRacer.Infrastructure.Sessions;

public sealed class InMemoryPlayerSessionStore : IPlayerSessionStore
{
    private readonly ConcurrentDictionary<string, LobbyPlayerSession> _sessions = new(StringComparer.Ordinal);

    public Task SaveAsync(LobbyPlayerSession session, CancellationToken cancellationToken)
    {
        _sessions[session.ReconnectToken] = session;
        return Task.CompletedTask;
    }

    public Task<LobbyPlayerSession?> GetByReconnectTokenAsync(string reconnectToken, CancellationToken cancellationToken)
    {
        _sessions.TryGetValue(reconnectToken, out var session);
        return Task.FromResult(session);
    }
}

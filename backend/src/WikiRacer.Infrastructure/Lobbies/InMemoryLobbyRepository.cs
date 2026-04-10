using System.Collections.Concurrent;
using WikiRacer.Application.Abstractions.Lobbies;
using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Infrastructure.Lobbies;

public sealed class InMemoryLobbyRepository : ILobbyRepository
{
    private readonly ConcurrentDictionary<string, Lobby> _lobbies = new(StringComparer.Ordinal);

    public Task AddAsync(Lobby lobby, CancellationToken cancellationToken)
    {
        _lobbies[lobby.PublicId.Value] = lobby;
        return Task.CompletedTask;
    }

    public Task<Lobby?> GetByPublicIdAsync(PublicLobbyId publicLobbyId, CancellationToken cancellationToken)
    {
        _lobbies.TryGetValue(publicLobbyId.Value, out var lobby);
        return Task.FromResult(lobby);
    }
}

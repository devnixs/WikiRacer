using System.Collections.Concurrent;
using WikiRacer.Application.Abstractions.Matches;
using WikiRacer.Domain.Lobbies;
using WikiRacer.Domain.Matches;

namespace WikiRacer.Infrastructure.Matches;

public sealed class InMemoryMatchRepository : IMatchRepository
{
    private readonly ConcurrentDictionary<MatchId, Match> _matches = new();
    private readonly ConcurrentDictionary<LobbyId, MatchId> _byLobbyId = new();

    public Task AddAsync(Match match, CancellationToken cancellationToken)
    {
        _matches[match.Id] = match;
        _byLobbyId[match.LobbyId] = match.Id;
        return Task.CompletedTask;
    }

    public Task<Match?> GetByIdAsync(MatchId matchId, CancellationToken cancellationToken)
    {
        _matches.TryGetValue(matchId, out var match);
        return Task.FromResult(match);
    }

    public Task<Match?> GetByLobbyIdAsync(LobbyId lobbyId, CancellationToken cancellationToken)
    {
        if (_byLobbyId.TryGetValue(lobbyId, out var matchId))
        {
            _matches.TryGetValue(matchId, out var match);
            return Task.FromResult(match);
        }

        return Task.FromResult<Match?>(null);
    }
}

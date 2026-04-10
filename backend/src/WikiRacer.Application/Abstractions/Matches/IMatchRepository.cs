using WikiRacer.Domain.Lobbies;
using WikiRacer.Domain.Matches;

namespace WikiRacer.Application.Abstractions.Matches;

public interface IMatchRepository
{
    Task AddAsync(Match match, CancellationToken cancellationToken);

    Task<Match?> GetByIdAsync(MatchId matchId, CancellationToken cancellationToken);

    Task<Match?> GetByLobbyIdAsync(LobbyId lobbyId, CancellationToken cancellationToken);
}

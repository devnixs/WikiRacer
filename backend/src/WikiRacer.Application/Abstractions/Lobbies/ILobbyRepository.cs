using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Application.Abstractions.Lobbies;

public interface ILobbyRepository
{
    Task AddAsync(Lobby lobby, CancellationToken cancellationToken);

    Task<Lobby?> GetByPublicIdAsync(PublicLobbyId publicLobbyId, CancellationToken cancellationToken);
}

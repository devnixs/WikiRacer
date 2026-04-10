using WikiRacer.Application.Lobbies;

namespace WikiRacer.Application.Abstractions.Sessions;

public interface IPlayerSessionStore
{
    Task SaveAsync(LobbyPlayerSession session, CancellationToken cancellationToken);

    Task<LobbyPlayerSession?> GetByReconnectTokenAsync(string reconnectToken, CancellationToken cancellationToken);
}

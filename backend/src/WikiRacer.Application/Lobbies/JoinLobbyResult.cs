using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Application.Lobbies;

public sealed record JoinLobbyResult(
    Lobby Lobby,
    LobbyPlayerSession Session,
    bool WasReconnect);

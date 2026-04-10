using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Application.Lobbies;

public sealed record CreateLobbyResult(
    Lobby Lobby,
    LobbyPlayerSession Session);

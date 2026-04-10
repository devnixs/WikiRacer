using WikiRacer.Contracts.Lobbies;

namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record LobbyUpdatedEvent(
    string LobbyId,
    int Revision,
    string Reason,
    LobbyStateView Lobby);

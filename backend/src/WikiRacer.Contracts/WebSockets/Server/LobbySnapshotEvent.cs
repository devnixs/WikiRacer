using WikiRacer.Contracts.Lobbies;

namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record LobbySnapshotEvent(
    LobbyStateView Lobby,
    bool IsResync);

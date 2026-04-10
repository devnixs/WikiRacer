using WikiRacer.Contracts.Lobbies;

namespace WikiRacer.Contracts.Requests;

public sealed record JoinLobbyResponse(
    string PlayerId,
    string ConnectionToken,
    string ReconnectToken,
    LobbyStateView Lobby);

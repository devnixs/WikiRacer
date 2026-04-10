using WikiRacer.Contracts.Lobbies;

namespace WikiRacer.Contracts.Requests;

public sealed record CreateLobbyResponse(
    string LobbyUrl,
    string PlayerId,
    string ReconnectToken,
    LobbyStateView Lobby);

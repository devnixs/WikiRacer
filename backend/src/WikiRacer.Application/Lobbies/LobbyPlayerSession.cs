using WikiRacer.Domain.Lobbies;
using WikiRacer.Domain.Players;

namespace WikiRacer.Application.Lobbies;

public sealed record LobbyPlayerSession(
    LobbyId LobbyId,
    PublicLobbyId PublicLobbyId,
    PlayerId PlayerId,
    string DisplayName,
    string ReconnectToken,
    string ConnectionToken);

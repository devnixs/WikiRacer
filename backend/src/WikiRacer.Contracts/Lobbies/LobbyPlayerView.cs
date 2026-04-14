namespace WikiRacer.Contracts.Lobbies;

public sealed record LobbyPlayerView(
    string PlayerId,
    string DisplayName,
    bool IsHost,
    bool IsConnected);

namespace WikiRacer.Contracts.Lobbies;

public sealed record LobbyStateView(
    string LobbyId,
    string PublicLobbyId,
    string Status,
    LobbySettingsView Settings,
    IReadOnlyList<LobbyPlayerView> Players,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    LobbyCountdownView? Countdown);

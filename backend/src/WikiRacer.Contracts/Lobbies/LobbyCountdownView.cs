namespace WikiRacer.Contracts.Lobbies;

public sealed record LobbyCountdownView(
    DateTimeOffset EndsAtUtc,
    int DurationSeconds);

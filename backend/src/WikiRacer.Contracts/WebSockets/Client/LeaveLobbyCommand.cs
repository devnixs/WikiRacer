namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record LeaveLobbyCommand(
    string LobbyId,
    string PlayerId);

namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record SetPlayerReadyCommand(
    string LobbyId,
    string PlayerId,
    bool IsReady);

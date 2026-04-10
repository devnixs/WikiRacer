namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record StartMatchCommand(
    string LobbyId,
    string RequestedByPlayerId);

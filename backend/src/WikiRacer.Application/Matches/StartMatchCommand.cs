namespace WikiRacer.Application.Matches;

public sealed record StartMatchCommand(
    string PublicLobbyId,
    string RequestedByPlayerId);

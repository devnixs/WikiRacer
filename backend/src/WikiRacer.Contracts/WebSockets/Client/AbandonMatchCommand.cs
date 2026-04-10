namespace WikiRacer.Contracts.WebSockets.Client;

public sealed record AbandonMatchCommand(
    string MatchId,
    string PlayerId,
    string? Reason = null);

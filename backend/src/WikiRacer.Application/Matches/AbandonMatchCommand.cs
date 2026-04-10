namespace WikiRacer.Application.Matches;

public sealed record AbandonMatchCommand(
    string MatchId,
    string PlayerId);

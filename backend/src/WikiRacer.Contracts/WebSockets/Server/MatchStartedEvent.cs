using WikiRacer.Contracts.Matches;

namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record MatchStartedEvent(
    MatchStateView Match);

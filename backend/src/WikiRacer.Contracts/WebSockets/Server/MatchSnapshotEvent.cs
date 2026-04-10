using WikiRacer.Contracts.Matches;

namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record MatchSnapshotEvent(
    MatchStateView Match,
    bool IsResync);

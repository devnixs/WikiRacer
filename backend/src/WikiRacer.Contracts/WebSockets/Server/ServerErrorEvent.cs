using WikiRacer.Contracts.Errors;

namespace WikiRacer.Contracts.WebSockets.Server;

public sealed record ServerErrorEvent(
    string? ScopeType,
    string? ScopeId,
    ErrorPayload Error);

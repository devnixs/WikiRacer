namespace WikiRacer.Contracts.Protocol;

public sealed record ServerMessageEnvelope<TPayload>(
    string Version,
    string MessageType,
    long Sequence,
    DateTimeOffset SentAtUtc,
    string? CorrelationId,
    TPayload Payload);

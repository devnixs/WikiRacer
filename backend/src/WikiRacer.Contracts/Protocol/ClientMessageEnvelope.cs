namespace WikiRacer.Contracts.Protocol;

public sealed record ClientMessageEnvelope<TPayload>(
    string Version,
    string MessageType,
    string MessageId,
    DateTimeOffset SentAtUtc,
    TPayload Payload);

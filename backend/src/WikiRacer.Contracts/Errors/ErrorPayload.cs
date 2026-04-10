namespace WikiRacer.Contracts.Errors;

public sealed record ErrorPayload(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null,
    string? RetryableReason = null);

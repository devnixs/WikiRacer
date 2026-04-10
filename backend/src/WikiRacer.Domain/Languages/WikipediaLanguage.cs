using WikiRacer.Domain.Common;

namespace WikiRacer.Domain.Languages;

public readonly record struct WikipediaLanguage
{
    public WikipediaLanguage(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value)).ToLowerInvariant();

        if (normalized.Length is < 2 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Wikipedia language codes must be between 2 and 10 characters.");
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static WikipediaLanguage French => new("fr");

    public static implicit operator string(WikipediaLanguage language) => language.Value;
}

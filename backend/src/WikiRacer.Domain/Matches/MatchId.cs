using WikiRacer.Domain.Common;

namespace WikiRacer.Domain.Matches;

public readonly record struct MatchId
{
    public MatchId(Guid value)
    {
        Value = Guard.AgainstEmpty(value, nameof(value));
    }

    public Guid Value { get; }

    public static MatchId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString("N");
}

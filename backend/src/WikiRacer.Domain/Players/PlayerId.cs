using WikiRacer.Domain.Common;

namespace WikiRacer.Domain.Players;

public readonly record struct PlayerId
{
    public PlayerId(Guid value)
    {
        Value = Guard.AgainstEmpty(value, nameof(value));
    }

    public Guid Value { get; }

    public static PlayerId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString("N");
}

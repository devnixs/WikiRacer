using WikiRacer.Domain.Common;

namespace WikiRacer.Domain.Lobbies;

public readonly record struct LobbyId
{
    public LobbyId(Guid value)
    {
        Value = Guard.AgainstEmpty(value, nameof(value));
    }

    public Guid Value { get; }

    public static LobbyId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString("N");
}

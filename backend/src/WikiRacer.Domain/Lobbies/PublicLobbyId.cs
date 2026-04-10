using System.Security.Cryptography;
using WikiRacer.Domain.Common;

namespace WikiRacer.Domain.Lobbies;

public readonly record struct PublicLobbyId
{
    public PublicLobbyId(string value)
    {
        Value = Guard.AgainstNullOrWhiteSpace(value, nameof(value)).ToUpperInvariant();
    }

    public string Value { get; }

    public static PublicLobbyId New()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> chars = stackalloc char[8];
        Span<byte> bytes = stackalloc byte[8];

        RandomNumberGenerator.Fill(bytes);

        for (var index = 0; index < chars.Length; index++)
        {
            chars[index] = alphabet[bytes[index] % alphabet.Length];
        }

        return new PublicLobbyId(new string(chars));
    }

    public override string ToString() => Value;

    public static implicit operator string(PublicLobbyId lobbyId) => lobbyId.Value;
}

using System.Security.Cryptography;
using WikiRacer.Application.Abstractions.Tokens;

namespace WikiRacer.Infrastructure.Tokens;

public sealed class SessionTokenFactory : ISessionTokenFactory
{
    public string Create()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}

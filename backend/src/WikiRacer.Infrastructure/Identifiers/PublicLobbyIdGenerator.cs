using WikiRacer.Application.Abstractions.Identifiers;
using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Infrastructure.Identifiers;

public sealed class PublicLobbyIdGenerator : IPublicLobbyIdGenerator
{
    public PublicLobbyId Create()
    {
        return PublicLobbyId.New();
    }
}

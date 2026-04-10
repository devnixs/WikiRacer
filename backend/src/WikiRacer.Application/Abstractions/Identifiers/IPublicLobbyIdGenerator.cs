using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Application.Abstractions.Identifiers;

public interface IPublicLobbyIdGenerator
{
    PublicLobbyId Create();
}

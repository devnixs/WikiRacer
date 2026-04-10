using WikiRacer.Application.Lobbies;
using WikiRacer.Infrastructure.Clock;
using WikiRacer.Infrastructure.Identifiers;
using WikiRacer.Infrastructure.Lobbies;
using WikiRacer.Infrastructure.Sessions;
using WikiRacer.Infrastructure.Tokens;

namespace WikiRacer.ArchitectureTests;

internal static class LobbyServiceTestsFactory
{
    public static LobbyService Create(InMemoryLobbyRepository? repository = null)
    {
        return new LobbyService(
            repository ?? new InMemoryLobbyRepository(),
            new InMemoryPlayerSessionStore(),
            new PublicLobbyIdGenerator(),
            new SessionTokenFactory(),
            new SystemClock());
    }
}

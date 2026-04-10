using System.Net;
using WikiRacer.Application.Lobbies;
using WikiRacer.Contracts.Errors;
using WikiRacer.Infrastructure.Clock;
using WikiRacer.Infrastructure.Identifiers;
using WikiRacer.Infrastructure.Lobbies;
using WikiRacer.Infrastructure.Sessions;
using WikiRacer.Infrastructure.Tokens;

namespace WikiRacer.ArchitectureTests;

public class LobbyServiceTests
{
    [Fact]
    public async Task Create_Should_Create_Host_And_Public_Lobby_Id()
    {
        var service = CreateService();

        var result = await service.CreateAsync(
            new CreateLobbyCommand("Raphael", "fr", "fr", 4, 600),
            CancellationToken.None);

        Assert.Equal("Raphael", result.Session.DisplayName);
        Assert.Single(result.Lobby.Players);
        Assert.Equal(8, result.Lobby.PublicId.Value.Length);
    }

    [Fact]
    public async Task Join_Should_Reconnect_Existing_Player_When_Reconnect_Token_Is_Used()
    {
        var service = CreateService();
        var createResult = await service.CreateAsync(
            new CreateLobbyCommand("Raphael", "fr", "fr", 4, 600),
            CancellationToken.None);

        var reconnectResult = await service.JoinAsync(
            new JoinLobbyCommand(createResult.Lobby.PublicId.Value, null, createResult.Session.ReconnectToken),
            CancellationToken.None);

        Assert.True(reconnectResult.WasReconnect);
        Assert.Equal(createResult.Session.PlayerId, reconnectResult.Session.PlayerId);
        Assert.NotEqual(createResult.Session.ConnectionToken, reconnectResult.Session.ConnectionToken);
    }

    [Fact]
    public async Task Join_Should_Reject_When_Lobby_Is_Full()
    {
        var service = CreateService();
        var createResult = await service.CreateAsync(
            new CreateLobbyCommand("Host", "fr", "fr", 1, 600),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<LobbyOperationException>(() =>
            service.JoinAsync(new JoinLobbyCommand(createResult.Lobby.PublicId.Value, "Guest", null), CancellationToken.None));

        Assert.Equal(ErrorCodes.LobbyFull, exception.ErrorCode);
        Assert.Equal((int)HttpStatusCode.Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateLanguage_Should_Allow_Host_To_Change_Lobby_Language()
    {
        var service = CreateService();
        var createResult = await service.CreateAsync(
            new CreateLobbyCommand("Host", "fr", "fr", 4, 600),
            CancellationToken.None);

        var updated = await service.UpdateLanguageAsync(
            new UpdateLobbyLanguageCommand(
                createResult.Lobby.PublicId.Value,
                createResult.Session.PlayerId.ToString(),
                "en"),
            CancellationToken.None);

        Assert.Equal("en", updated.Lobby.Settings.Language.Value);
        Assert.Equal("en", updated.Lobby.Settings.UiLanguage.Value);
    }

    [Fact]
    public async Task UpdateLanguage_Should_Reject_Non_Host_Player()
    {
        var service = CreateService();
        var createResult = await service.CreateAsync(
            new CreateLobbyCommand("Host", "fr", "fr", 4, 600),
            CancellationToken.None);
        var joinResult = await service.JoinAsync(
            new JoinLobbyCommand(createResult.Lobby.PublicId.Value, "Guest", null),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<LobbyOperationException>(() =>
            service.UpdateLanguageAsync(
                new UpdateLobbyLanguageCommand(
                    createResult.Lobby.PublicId.Value,
                    joinResult.Session.PlayerId.ToString(),
                    "en"),
                CancellationToken.None));

        Assert.Equal(ErrorCodes.UnauthorizedAction, exception.ErrorCode);
        Assert.Equal((int)HttpStatusCode.Forbidden, exception.StatusCode);
    }

    private static LobbyService CreateService()
    {
        return LobbyServiceTestsFactory.Create();
    }
}

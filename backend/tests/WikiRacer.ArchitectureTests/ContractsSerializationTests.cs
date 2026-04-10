using System.Text.Json;
using WikiRacer.Contracts;
using WikiRacer.Contracts.Errors;
using WikiRacer.Contracts.Json;
using WikiRacer.Contracts.Lobbies;
using WikiRacer.Contracts.Matches;
using WikiRacer.Contracts.Protocol;
using WikiRacer.Contracts.WebSockets.Client;
using WikiRacer.Contracts.WebSockets.Server;

namespace WikiRacer.ArchitectureTests;

public class ContractsSerializationTests
{
    [Fact]
    public void Client_Envelope_Should_Serialize_ReportProgress_Command()
    {
        var envelope = new ClientMessageEnvelope<ReportProgressCommand>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.ReportProgress,
            "msg-001",
            new DateTimeOffset(2026, 4, 10, 12, 0, 0, TimeSpan.Zero),
            new ReportProgressCommand("match-1", "player-1", "Paris", 4, new DateTimeOffset(2026, 4, 10, 12, 0, 1, TimeSpan.Zero)));

        var json = JsonSerializer.Serialize(envelope, ContractsJson.Default);

        Assert.Contains("\"version\":\"1.0\"", json);
        Assert.Contains("\"messageType\":\"match.progress.report\"", json);
        Assert.Contains("\"currentArticleTitle\":\"Paris\"", json);
    }

    [Fact]
    public void Server_Envelope_Should_Roundtrip_Lobby_Snapshot()
    {
        var envelope = new ServerMessageEnvelope<LobbySnapshotEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.LobbySnapshot,
            42,
            new DateTimeOffset(2026, 4, 10, 12, 5, 0, TimeSpan.Zero),
            "msg-join-1",
            new LobbySnapshotEvent(
                new LobbyStateView(
                    "lobby-1",
                    "ABCD2345",
                    "waiting",
                    new LobbySettingsView(
                        "fr",
                        "fr",
                        600,
                        8,
                        new ArticleReference("Paris", "Paris", "/wiki/Paris"),
                        new ArticleReference("Lyon", "Lyon", "/wiki/Lyon"),
                        ArticleSelectionMode.Manual,
                        ArticleSelectionMode.Random),
                    new[]
                    {
                        new LobbyPlayerView("player-1", "Raphael", true, true, true)
                    },
                    7,
                    new DateTimeOffset(2026, 4, 10, 11, 59, 0, TimeSpan.Zero),
                    new LobbyCountdownView(
                        new DateTimeOffset(2026, 4, 10, 12, 5, 3, TimeSpan.Zero),
                        3)),
                false));

        var json = JsonSerializer.Serialize(envelope, ContractsJson.Default);
        var roundtrip = JsonSerializer.Deserialize<ServerMessageEnvelope<LobbySnapshotEvent>>(json, ContractsJson.Default);

        Assert.NotNull(roundtrip);
        Assert.Equal(42, roundtrip.Sequence);
        Assert.Equal("ABCD2345", roundtrip.Payload.Lobby.PublicLobbyId);
        Assert.Equal(ArticleSelectionMode.Random, roundtrip.Payload.Lobby.Settings.TargetSelectionMode);
    }

    [Fact]
    public void Error_Event_Should_Use_Stable_Machine_Readable_Code()
    {
        var envelope = new ServerMessageEnvelope<ServerErrorEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.Error,
            99,
            new DateTimeOffset(2026, 4, 10, 12, 10, 0, TimeSpan.Zero),
            "msg-err-1",
            new ServerErrorEvent(
                "lobby",
                "ABCD2345",
                new ErrorPayload(
                    ErrorCodes.LobbyNotFound,
                    "Lobby was not found.")));

        var json = JsonSerializer.Serialize(envelope, ContractsJson.Default);

        Assert.Contains("\"code\":\"lobby_not_found\"", json);
        Assert.Contains("\"messageType\":\"error\"", json);
    }

    [Fact]
    public void Match_Snapshot_Should_Serialize_String_Enums()
    {
        var envelope = new ServerMessageEnvelope<MatchSnapshotEvent>(
            ProtocolVersions.V1,
            ProtocolMessageTypes.MatchSnapshot,
            501,
            new DateTimeOffset(2026, 4, 10, 12, 15, 0, TimeSpan.Zero),
            null,
            new MatchSnapshotEvent(
                new MatchStateView(
                    "match-1",
                    "lobby-1",
                    "fr",
                    new ArticleReference("Paris", "Paris", "/wiki/Paris"),
                    new ArticleReference("Lyon", "Lyon", "/wiki/Lyon"),
                    "active",
                    new DateTimeOffset(2026, 4, 10, 12, 14, 0, TimeSpan.Zero),
                    new[]
                    {
                        new MatchPlayerStateView("player-1", "Raphael", MatchPlayerRaceStatus.Active, "Paris", 0, null, null, true)
                    },
                    501),
                false));

        var json = JsonSerializer.Serialize(envelope, ContractsJson.Default);

        Assert.Contains("\"status\":\"active\"", json);
    }
}

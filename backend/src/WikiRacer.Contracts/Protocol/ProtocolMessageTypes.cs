namespace WikiRacer.Contracts.Protocol;

public static class ProtocolMessageTypes
{
    public const string JoinLobby = "lobby.join";
    public const string LeaveLobby = "lobby.leave";
    public const string UpdateLobbySettings = "lobby.settings.update";
    public const string SetPlayerReady = "lobby.player.ready";
    public const string StartMatch = "match.start";
    public const string ReportProgress = "match.progress.report";
    public const string AbandonMatch = "match.abandon";
    public const string Reconnect = "session.reconnect";
    public const string RequestResync = "session.resync.request";

    public const string LobbySnapshot = "lobby.snapshot";
    public const string LobbyUpdated = "lobby.updated";
    public const string MatchSnapshot = "match.snapshot";
    public const string MatchStarted = "match.started";
    public const string PlayerProgressed = "match.player.progressed";
    public const string PlayerFinished = "match.player.finished";
    public const string PlayerAbandoned = "match.player.abandoned";
    public const string ReconnectAccepted = "session.reconnect.accepted";
    public const string ResyncRequired = "session.resync.required";
    public const string Error = "error";
}

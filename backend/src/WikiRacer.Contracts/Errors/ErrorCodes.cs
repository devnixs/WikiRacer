namespace WikiRacer.Contracts.Errors;

public static class ErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string ArticleNotFound = "article_not_found";
    public const string LobbyNotFound = "lobby_not_found";
    public const string LobbyFull = "lobby_full";
    public const string LobbyNotJoinable = "lobby_not_joinable";
    public const string MatchNotFound = "match_not_found";
    public const string PlayerNotFound = "player_not_found";
    public const string DuplicateMessage = "duplicate_message";
    public const string ReconnectRejected = "reconnect_rejected";
    public const string ResyncRequired = "resync_required";
    public const string UnauthorizedAction = "unauthorized_action";
    public const string InternalError = "internal_error";
}

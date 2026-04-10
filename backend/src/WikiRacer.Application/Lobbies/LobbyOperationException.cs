namespace WikiRacer.Application.Lobbies;

public sealed class LobbyOperationException(string errorCode, string message, int statusCode) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;

    public int StatusCode { get; } = statusCode;
}

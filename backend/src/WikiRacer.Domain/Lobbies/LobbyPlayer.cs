using WikiRacer.Domain.Common;
using WikiRacer.Domain.Players;

namespace WikiRacer.Domain.Lobbies;

public sealed class LobbyPlayer
{
    public LobbyPlayer(PlayerId playerId, string displayName, bool isHost)
    {
        PlayerId = playerId;
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName));
        IsHost = isHost;
        IsConnected = true;
    }

    public PlayerId PlayerId { get; }

    public string DisplayName { get; }

    public bool IsHost { get; }

    public bool IsConnected { get; private set; }

    public void MarkConnected()
    {
        IsConnected = true;
    }

    public void MarkDisconnected()
    {
        IsConnected = false;
    }
}

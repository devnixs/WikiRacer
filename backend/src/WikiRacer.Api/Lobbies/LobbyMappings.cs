using WikiRacer.Application.Lobbies;
using WikiRacer.Contracts;
using WikiRacer.Contracts.Lobbies;
using WikiRacer.Domain.Lobbies;

namespace WikiRacer.Api.Lobbies;

internal static class LobbyMappings
{
    public static LobbyStateView ToContract(this Lobby lobby)
    {
        return new LobbyStateView(
            lobby.Id.ToString(),
            lobby.PublicId.Value,
            lobby.Status.ToString().ToLowerInvariant(),
            new LobbySettingsView(
                lobby.Settings.Language.Value,
                lobby.Settings.UiLanguage.Value,
                lobby.Settings.TimeLimitSeconds,
                lobby.Settings.PlayerCap,
                lobby.Settings.StartArticle is null
                    ? null
                    : new ArticleReference(
                        lobby.Settings.StartArticle.Title.Value,
                        lobby.Settings.StartArticle.DisplayTitle,
                        lobby.Settings.StartArticle.CanonicalPath),
                lobby.Settings.TargetArticle is null
                    ? null
                    : new ArticleReference(
                        lobby.Settings.TargetArticle.Title.Value,
                        lobby.Settings.TargetArticle.DisplayTitle,
                        lobby.Settings.TargetArticle.CanonicalPath),
                (Contracts.Lobbies.ArticleSelectionMode)lobby.Settings.StartSelectionMode,
                (Contracts.Lobbies.ArticleSelectionMode)lobby.Settings.TargetSelectionMode),
            lobby.Players.Select(player => new LobbyPlayerView(
                player.PlayerId.ToString(),
                player.DisplayName,
                player.IsHost,
                player.IsConnected)).ToArray(),
            lobby.Revision,
            lobby.CreatedAtUtc,
            lobby.CountdownEndsAtUtc is null
                ? null
                : new LobbyCountdownView(
                    lobby.CountdownEndsAtUtc.Value,
                    3));
    }
}

namespace WikiRacer.Application.Articles;

public sealed record UpdateLobbyArticleCommand(
    string PublicLobbyId,
    string PlayerId,
    string Slot,
    string Title);

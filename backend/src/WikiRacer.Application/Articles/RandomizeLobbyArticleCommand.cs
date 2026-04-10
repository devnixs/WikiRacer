namespace WikiRacer.Application.Articles;

public sealed record RandomizeLobbyArticleCommand(
    string PublicLobbyId,
    string PlayerId,
    string Slot);

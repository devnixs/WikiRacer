namespace WikiRacer.Contracts.Requests;

public sealed record UpdateLobbyArticleRequest(
    string PlayerId,
    string Title);

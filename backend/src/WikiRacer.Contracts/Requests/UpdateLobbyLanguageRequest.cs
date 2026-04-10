namespace WikiRacer.Contracts.Requests;

public sealed record UpdateLobbyLanguageRequest(
    string PlayerId,
    string Language);

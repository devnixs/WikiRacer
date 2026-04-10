using WikiRacer.Application.Lobbies;

namespace WikiRacer.Application.Abstractions.Lobbies;

public interface ILobbyService
{
    Task<CreateLobbyResult> CreateAsync(CreateLobbyCommand command, CancellationToken cancellationToken);

    Task<LobbySnapshotResult> GetByPublicIdAsync(string publicLobbyId, CancellationToken cancellationToken);

    Task<JoinLobbyResult> JoinAsync(JoinLobbyCommand command, CancellationToken cancellationToken);

    Task<UpdateLobbyLanguageResult> UpdateLanguageAsync(UpdateLobbyLanguageCommand command, CancellationToken cancellationToken);

    Task<UpdateLobbyReadyResult> UpdateReadyAsync(UpdateLobbyReadyCommand command, CancellationToken cancellationToken);
}

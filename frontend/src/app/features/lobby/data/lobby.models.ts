export interface ArticleReference {
  title: string;
  displayTitle: string;
  canonicalPath: string;
}

export interface LobbyPlayerView {
  playerId: string;
  displayName: string;
  isHost: boolean;
  isReady: boolean;
  isConnected: boolean;
}

export interface LobbySettingsView {
  language: string;
  uiLanguage: string;
  timeLimitSeconds: number | null;
  playerCap: number;
  startArticle: ArticleReference | null;
  targetArticle: ArticleReference | null;
  startSelectionMode: 'manual' | 'random';
  targetSelectionMode: 'manual' | 'random';
}

export interface LobbyCountdownView {
  endsAtUtc: string;
  durationSeconds: number;
}

export interface LobbyStateView {
  lobbyId: string;
  publicLobbyId: string;
  status: string;
  settings: LobbySettingsView;
  players: LobbyPlayerView[];
  revision: number;
  createdAtUtc: string;
  countdown: LobbyCountdownView | null;
}

export interface CreateLobbyRequest {
  displayName: string;
  language: string;
  uiLanguage: string;
  playerCap: number;
  timeLimitSeconds: number | null;
}

export interface CreateLobbyResponse {
  lobbyUrl: string;
  playerId: string;
  reconnectToken: string;
  lobby: LobbyStateView;
}

export interface JoinLobbyRequest {
  displayName: string;
  reconnectToken?: string | null;
}

export interface JoinLobbyResponse {
  playerId: string;
  connectionToken: string;
  reconnectToken: string;
  lobby: LobbyStateView;
}

export interface UpdateLobbyLanguageRequest {
  playerId: string;
  language: string;
}

export interface ErrorPayload {
  code: string;
  message: string;
}

export interface ArticleSearchResponse {
  title: string;
  displayTitle: string;
  canonicalPath: string;
  description?: string | null;
}

export interface UpdateLobbyArticleRequest {
  playerId: string;
  title: string;
}

export interface RandomizeLobbyArticleRequest {
  playerId: string;
}

export interface UpdateLobbyReadyRequest {
  isReady: boolean;
}

export interface LobbySession {
  publicLobbyId: string;
  playerId: string;
  displayName: string;
  reconnectToken: string;
  connectionToken?: string;
}

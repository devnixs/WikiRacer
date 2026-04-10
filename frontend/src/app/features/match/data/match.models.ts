import { ArticleReference } from '../../lobby/data/lobby.models';

export interface MatchPlayerStateView {
  playerId: string;
  displayName: string;
  status: 'active' | 'finished' | 'abandoned' | 'disconnected';
  currentArticleTitle: string | null;
  hopCount: number;
  finishTime: string | null;
  placement: number | null;
  isConnected: boolean;
}

export interface MatchStateView {
  matchId: string;
  lobbyId: string;
  language: string;
  startArticle: ArticleReference;
  targetArticle: ArticleReference;
  status: 'inProgress' | 'finished';
  startedAtUtc: string;
  players: MatchPlayerStateView[];
  timelineSequence: number;
}

export interface StartMatchRequest {
  requestedByPlayerId: string;
}

export interface ReportMatchProgressRequest {
  playerId: string;
  currentArticleTitle: string;
  reportedAtUtc: string;
}

export interface AbandonMatchRequest {
  playerId: string;
}

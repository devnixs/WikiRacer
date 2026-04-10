import { ArticleReference } from '../../lobby/data/lobby.models';

export type SoloRaceStatus = 'active' | 'finished';

export interface SoloRaceSnapshot {
  publicLobbyId: string;
  language: string;
  startArticle: ArticleReference;
  targetArticle: ArticleReference;
  currentArticleTitle: string;
  hopCount: number;
  startedAtUtc: string;
  finishedAtUtc: string | null;
  status: SoloRaceStatus;
  visitedTitles: string[];
}

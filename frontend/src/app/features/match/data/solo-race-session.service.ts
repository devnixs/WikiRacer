import { Injectable } from '@angular/core';
import { ArticleReference } from '../../lobby/data/lobby.models';
import { SoloRaceSnapshot } from './solo-race.models';

@Injectable({ providedIn: 'root' })
export class SoloRaceSessionService {
  private static readonly StorageKey = 'wiki-racer.solo-race-sessions';

  get(publicLobbyId: string): SoloRaceSnapshot | null {
    return this.read()[publicLobbyId] ?? null;
  }

  start(publicLobbyId: string, language: string, startArticle: ArticleReference, targetArticle: ArticleReference): SoloRaceSnapshot {
    const startedAtUtc = new Date().toISOString();
    const isFinishedImmediately = startArticle.title === targetArticle.title;
    const snapshot: SoloRaceSnapshot = {
      publicLobbyId,
      language,
      startArticle,
      targetArticle,
      currentArticleTitle: startArticle.title,
      hopCount: 0,
      startedAtUtc,
      finishedAtUtc: isFinishedImmediately ? startedAtUtc : null,
      status: isFinishedImmediately ? 'finished' : 'active',
      visitedTitles: [startArticle.title]
    };

    this.save(snapshot);
    return snapshot;
  }

  save(snapshot: SoloRaceSnapshot): void {
    const sessions = this.read();
    sessions[snapshot.publicLobbyId] = snapshot;
    this.write(sessions);
  }

  clear(publicLobbyId: string): void {
    const sessions = this.read();
    delete sessions[publicLobbyId];
    this.write(sessions);
  }

  private read(): Record<string, SoloRaceSnapshot> {
    if (typeof sessionStorage === 'undefined') {
      return {};
    }

    const raw = sessionStorage.getItem(SoloRaceSessionService.StorageKey);

    if (!raw) {
      return {};
    }

    try {
      return JSON.parse(raw) as Record<string, SoloRaceSnapshot>;
    } catch {
      return {};
    }
  }

  private write(value: Record<string, SoloRaceSnapshot>): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    sessionStorage.setItem(SoloRaceSessionService.StorageKey, JSON.stringify(value));
  }
}

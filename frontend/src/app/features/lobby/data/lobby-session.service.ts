import { Injectable } from '@angular/core';
import { LobbySession } from './lobby.models';

@Injectable({ providedIn: 'root' })
export class LobbySessionService {
  private static readonly StorageKey = 'wiki-racer.lobby-sessions';

  get(publicLobbyId: string): LobbySession | null {
    return this.read()[publicLobbyId] ?? null;
  }

  save(session: LobbySession): void {
    const sessions = this.read();
    sessions[session.publicLobbyId] = session;
    this.write(sessions);
  }

  remove(publicLobbyId: string): void {
    const sessions = this.read();
    delete sessions[publicLobbyId];
    this.write(sessions);
  }

  private read(): Record<string, LobbySession> {
    if (typeof sessionStorage === 'undefined') {
      return {};
    }

    const raw = sessionStorage.getItem(LobbySessionService.StorageKey);

    if (!raw) {
      return {};
    }

    try {
      return JSON.parse(raw) as Record<string, LobbySession>;
    } catch {
      return {};
    }
  }

  private write(value: Record<string, LobbySession>): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    sessionStorage.setItem(LobbySessionService.StorageKey, JSON.stringify(value));
  }
}

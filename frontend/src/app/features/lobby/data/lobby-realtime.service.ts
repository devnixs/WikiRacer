import { Injectable } from '@angular/core';
import { LobbyStateView } from './lobby.models';

interface LobbySnapshotPayload {
  lobby: LobbyStateView;
  isResync: boolean;
}

interface LobbyUpdatedPayload {
  lobbyId: string;
  revision: number;
  reason: string;
  lobby: LobbyStateView;
}

interface ServerEnvelope<TPayload> {
  version: string;
  messageType: string;
  sequence: number;
  sentAtUtc: string;
  correlationId?: string | null;
  payload: TPayload;
}

export interface LobbyRealtimeCallbacks {
  onLobbyChanged?: (lobby: LobbyStateView) => void;
  onMessage?: (messageType: string, payload: unknown) => void;
  onError: (message: string) => void;
}

@Injectable({ providedIn: 'root' })
export class LobbyRealtimeService {
  private socket: WebSocket | null = null;

  connect(
    publicLobbyId: string,
    reconnectToken: string,
    callbacks: LobbyRealtimeCallbacks
  ): void {
    this.disconnect();

    if (typeof WebSocket === 'undefined') {
      return;
    }

    const protocol = window.location.protocol === 'https:' ? 'wss' : 'ws';
    const url = `${protocol}://${window.location.host}/ws/lobbies/${publicLobbyId}?reconnectToken=${encodeURIComponent(reconnectToken)}`;
    this.socket = new WebSocket(url);

    this.socket.onmessage = (event) => {
      try {
        const envelope = JSON.parse(event.data) as ServerEnvelope<unknown>;

        if (envelope.messageType === 'lobby.snapshot') {
          callbacks.onLobbyChanged?.((envelope.payload as LobbySnapshotPayload).lobby);
        } else if (envelope.messageType === 'lobby.updated') {
          callbacks.onLobbyChanged?.((envelope.payload as LobbyUpdatedPayload).lobby);
        }

        callbacks.onMessage?.(envelope.messageType, envelope.payload);
      } catch {
        callbacks.onError('Realtime updates could not be parsed.');
      }
    };

    this.socket.onerror = () => {
      callbacks.onError('Realtime updates are temporarily unavailable.');
    };
  }

  disconnect(): void {
    if (!this.socket) {
      return;
    }

    this.socket.onmessage = null;
    this.socket.onerror = null;
    this.socket.close();
    this.socket = null;
  }
}

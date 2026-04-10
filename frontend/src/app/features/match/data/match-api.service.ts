import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AbandonMatchRequest, MatchStateView, ReportMatchProgressRequest, StartMatchRequest } from './match.models';

@Injectable({ providedIn: 'root' })
export class MatchApiService {
  private readonly httpClient = inject(HttpClient);

  async startMatch(publicLobbyId: string, request: StartMatchRequest): Promise<MatchStateView> {
    return firstValueFrom(this.httpClient.post<MatchStateView>(`/api/lobbies/${publicLobbyId}/match/start`, request));
  }

  async getByLobby(publicLobbyId: string): Promise<MatchStateView> {
    return firstValueFrom(this.httpClient.get<MatchStateView>(`/api/lobbies/${publicLobbyId}/match`));
  }

  async reportProgress(matchId: string, request: ReportMatchProgressRequest): Promise<MatchStateView> {
    return firstValueFrom(this.httpClient.post<MatchStateView>(`/api/matches/${matchId}/progress`, request));
  }

  async abandon(matchId: string, request: AbandonMatchRequest): Promise<MatchStateView> {
    return firstValueFrom(this.httpClient.post<MatchStateView>(`/api/matches/${matchId}/abandon`, request));
  }
}

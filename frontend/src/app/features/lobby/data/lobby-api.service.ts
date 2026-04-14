import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  ArticleSearchResponse,
  CreateLobbyRequest,
  CreateLobbyResponse,
  JoinLobbyRequest,
  JoinLobbyResponse,
  LobbyStateView,
  RandomizeLobbyArticleRequest,
  UpdateLobbyArticleRequest,
  UpdateLobbyLanguageRequest
} from './lobby.models';

@Injectable({ providedIn: 'root' })
export class LobbyApiService {
  private readonly httpClient = inject(HttpClient);

  async createLobby(request: CreateLobbyRequest): Promise<CreateLobbyResponse> {
    return firstValueFrom(this.httpClient.post<CreateLobbyResponse>('/api/lobbies', request));
  }

  async getLobby(publicLobbyId: string): Promise<LobbyStateView> {
    return firstValueFrom(this.httpClient.get<LobbyStateView>(`/api/lobbies/${publicLobbyId}`));
  }

  async joinLobby(publicLobbyId: string, request: JoinLobbyRequest): Promise<JoinLobbyResponse> {
    return firstValueFrom(this.httpClient.post<JoinLobbyResponse>(`/api/lobbies/${publicLobbyId}/join`, request));
  }

  async updateLanguage(publicLobbyId: string, request: UpdateLobbyLanguageRequest): Promise<LobbyStateView> {
    return firstValueFrom(this.httpClient.put<LobbyStateView>(`/api/lobbies/${publicLobbyId}/language`, request));
  }

  async searchArticles(language: string, query: string): Promise<ArticleSearchResponse[]> {
    const params = new HttpParams()
      .set('language', language)
      .set('query', query);

    return firstValueFrom(this.httpClient.get<ArticleSearchResponse[]>('/api/articles/search', { params }));
  }

  async updateArticle(publicLobbyId: string, slot: 'start' | 'target', request: UpdateLobbyArticleRequest): Promise<LobbyStateView> {
    return firstValueFrom(this.httpClient.put<LobbyStateView>(`/api/lobbies/${publicLobbyId}/articles/${slot}`, request));
  }

  async randomizeArticle(publicLobbyId: string, slot: 'start' | 'target', request: RandomizeLobbyArticleRequest): Promise<LobbyStateView> {
    return firstValueFrom(this.httpClient.post<LobbyStateView>(`/api/lobbies/${publicLobbyId}/articles/${slot}/randomize`, request));
  }
}

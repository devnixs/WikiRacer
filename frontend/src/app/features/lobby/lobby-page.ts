import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PageFrameComponent } from '../../shared/layout/page-frame';
import { ChipComponent } from '../../shared/ui/chip';
import { FeatureCardComponent } from '../../shared/ui/feature-card';
import { MetricTileComponent } from '../../shared/ui/metric-tile';
import { LobbyApiService } from './data/lobby-api.service';
import { ArticleSearchResponse, ErrorPayload, JoinLobbyResponse, LobbyStateView } from './data/lobby.models';
import { LobbyRealtimeService } from './data/lobby-realtime.service';
import { LobbySessionService } from './data/lobby-session.service';
import { LocalizationService } from '../../core/localization/localization.service';
import { MatchApiService } from '../match/data/match-api.service';

@Component({
  selector: 'app-lobby-page',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    PageFrameComponent,
    FeatureCardComponent,
    MetricTileComponent,
    ChipComponent
  ],
  templateUrl: './lobby-page.html',
  styleUrl: './lobby-page.scss'
})
export class LobbyPageComponent implements OnInit, OnDestroy {
  protected joinName = '';
  protected readonly publicLobbyId = signal<string | null>(null);
  protected readonly lobby = signal<LobbyStateView | null>(null);
  protected readonly joinState = signal<JoinLobbyResponse | null>(null);
  protected readonly entryError = signal<string | null>(null);
  protected readonly joinError = signal<string | null>(null);
  protected readonly languageError = signal<string | null>(null);
  protected readonly articleError = signal<string | null>(null);
  protected readonly realtimeError = signal<string | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly isJoining = signal(false);
  protected readonly isUpdatingLanguage = signal(false);
  protected readonly isUpdatingReady = signal(false);
  protected readonly isSearchingStart = signal(false);
  protected readonly isSearchingTarget = signal(false);
  protected readonly isRandomizingStart = signal(false);
  protected readonly isRandomizingTarget = signal(false);
  protected readonly copiedShareLink = signal(false);
  protected readonly sessionDisplayName = computed(() => this.session()?.displayName ?? '');
  protected readonly shareUrl = computed(() =>
    this.publicLobbyId() ? `${window.location.origin}/lobby/${this.publicLobbyId()}` : '');
  protected readonly startSearch = signal('');
  protected readonly targetSearch = signal('');
  protected readonly startSuggestions = signal<ArticleSearchResponse[]>([]);
  protected readonly targetSuggestions = signal<ArticleSearchResponse[]>([]);
  protected readonly countdownSeconds = signal(0);
  protected readonly canStartMultiplayer = computed(() => {
    const lobby = this.lobby();

    return !!lobby
      && lobby.status === 'waiting'
      && !!lobby.settings.startArticle
      && !!lobby.settings.targetArticle
      && lobby.players.length > 0
      && lobby.players.every((player) => player.isConnected && player.isReady);
  });
  protected readonly isHost = computed(() => {
    const lobby = this.lobby();
    const playerId = this.session()?.playerId;

    if (!lobby || !playerId) {
      return false;
    }

    return lobby.players.some((player) => player.playerId === playerId && player.isHost);
  });
  protected selectedLanguage = 'fr';
  protected readonly selectedArticlesSummary = computed(() => {
    const lobby = this.lobby();

    if (!lobby) {
      return '';
    }

    const source = this.toArticleSummary(
      lobby.settings.startArticle?.displayTitle,
      lobby.settings.startSelectionMode
    );
    const target = this.toArticleSummary(
      lobby.settings.targetArticle?.displayTitle,
      lobby.settings.targetSelectionMode
    );
    return `${this.localization.t('lobby.source')}: ${source}. ${this.localization.t('lobby.target')}: ${target}.`;
  });
  protected readonly currentPlayerReady = computed(() => {
    const lobby = this.lobby();
    const playerId = this.session()?.playerId;

    return lobby?.players.find((player) => player.playerId === playerId)?.isReady ?? false;
  });
  protected readonly playersSummary = computed(() => {
    const lobby = this.lobby();

    if (!lobby) {
      return '';
    }

    return lobby.players
      .map((player) => `${player.displayName} (${player.isReady ? this.localization.t('lobby.ready') : this.localization.t('lobby.notReady')})`)
      .join(', ');
  });

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly lobbyApi = inject(LobbyApiService);
  private readonly matchApi = inject(MatchApiService);
  private readonly lobbyRealtime = inject(LobbyRealtimeService);
  private readonly lobbySessionService = inject(LobbySessionService);
  private readonly session = signal<ReturnType<LobbySessionService['get']>>(null);
  protected readonly localization = inject(LocalizationService);
  private startSearchTimer: number | null = null;
  private targetSearchTimer: number | null = null;
  private countdownTimer: number | null = null;

  ngOnInit(): void {
    this.route.paramMap.subscribe(async (params) => {
      const publicLobbyId = params.get('publicLobbyId');
      this.publicLobbyId.set(publicLobbyId);
      this.joinState.set(null);
      this.entryError.set(null);
      this.joinError.set(null);
      this.languageError.set(null);
      this.articleError.set(null);
      this.realtimeError.set(null);
      this.copiedShareLink.set(false);
      this.isRandomizingStart.set(false);
      this.isRandomizingTarget.set(false);
      this.lobbyRealtime.disconnect();
      this.stopCountdownTimer();

      if (!publicLobbyId) {
        this.isLoading.set(false);
        this.lobby.set(null);
        return;
      }

      this.isLoading.set(true);
      this.session.set(this.lobbySessionService.get(publicLobbyId));

      try {
        const lobby = await this.lobbyApi.getLobby(publicLobbyId);
        this.applyLobbySnapshot(lobby);

        if (this.session()) {
          await this.joinLobby(true);
        }
      } catch (error) {
        this.entryError.set(this.readErrorMessage(error, 'Lobby could not be loaded.'));
        this.lobby.set(null);
      } finally {
        this.isLoading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.lobbyRealtime.disconnect();
    this.stopCountdownTimer();
  }

  protected async joinLobby(isAutomatic = false): Promise<void> {
    const publicLobbyId = this.publicLobbyId();

    if (!publicLobbyId || this.isJoining()) {
      return;
    }

    const currentSession = this.session();
    const displayName = isAutomatic ? currentSession?.displayName ?? '' : this.joinName.trim();

    if (!isAutomatic && !displayName) {
      return;
    }

    this.isJoining.set(true);
    this.joinError.set(null);

    try {
      const response = await this.lobbyApi.joinLobby(publicLobbyId, {
        displayName,
        reconnectToken: currentSession?.reconnectToken ?? null
      });

      this.joinState.set(response);
      this.lobby.set(response.lobby);
      this.session.set({
        publicLobbyId,
        playerId: response.playerId,
        displayName: displayName || currentSession?.displayName || '',
        reconnectToken: response.reconnectToken,
        connectionToken: response.connectionToken
      });
      this.lobbySessionService.save(this.session()!);
      this.applyLobbySnapshot(response.lobby);
      this.connectRealtime(publicLobbyId, response.reconnectToken);
      this.joinName = '';
    } catch (error) {
      if (isAutomatic) {
        this.lobbySessionService.remove(publicLobbyId);
        this.session.set(null);
      }

      this.joinError.set(this.readErrorMessage(error, 'Lobby could not be joined.'));
    } finally {
      this.isJoining.set(false);
    }
  }

  protected async toggleReady(): Promise<void> {
    const publicLobbyId = this.publicLobbyId();
    const session = this.session();

    if (!publicLobbyId || !session?.playerId || this.isUpdatingReady()) {
      return;
    }

    this.isUpdatingReady.set(true);
    this.joinError.set(null);

    try {
      const lobby = await this.lobbyApi.updateReady(publicLobbyId, session.playerId, {
        isReady: !this.currentPlayerReady()
      });

      this.applyLobbySnapshot(lobby);
    } catch (error) {
      this.joinError.set(this.readErrorMessage(error, this.localization.t('lobby.realtimeError')));
    } finally {
      this.isUpdatingReady.set(false);
    }
  }

  protected async startMultiplayerMatch(): Promise<void> {
    const publicLobbyId = this.publicLobbyId();
    const session = this.session();

    if (!publicLobbyId || !session?.playerId || !this.canStartMultiplayer()) {
      return;
    }

    this.joinError.set(null);

    try {
      await this.matchApi.startMatch(publicLobbyId, {
        requestedByPlayerId: session.playerId
      });

      await this.router.navigate(['/match', publicLobbyId], {
        queryParams: { mode: 'multiplayer' }
      });
    } catch (error) {
      this.joinError.set(this.readErrorMessage(error, this.localization.t('match.multiplayerStartError')));
    }
  }

  protected async updateLanguage(): Promise<void> {
    const publicLobbyId = this.publicLobbyId();
    const session = this.session();

    if (!publicLobbyId || !session?.playerId || this.isUpdatingLanguage() || !this.isHost()) {
      return;
    }

    this.isUpdatingLanguage.set(true);
    this.languageError.set(null);

    try {
      const lobby = await this.lobbyApi.updateLanguage(publicLobbyId, {
        playerId: session.playerId,
        language: this.selectedLanguage
      });

      this.applyLobbySnapshot(lobby);
    } catch (error) {
      this.languageError.set(this.readErrorMessage(error, this.localization.t('lobby.changeLanguageError')));
    } finally {
      this.isUpdatingLanguage.set(false);
    }
  }

  protected onSearchInput(slot: 'start' | 'target', value: string): void {
    if (slot === 'start') {
      this.startSearch.set(value);
      this.startSuggestions.set([]);
      this.clearTimer('start');
      this.startSearchTimer = window.setTimeout(() => void this.searchArticles(slot, value), 250);
    } else {
      this.targetSearch.set(value);
      this.targetSuggestions.set([]);
      this.clearTimer('target');
      this.targetSearchTimer = window.setTimeout(() => void this.searchArticles(slot, value), 250);
    }
  }

  protected async selectArticle(slot: 'start' | 'target', title: string): Promise<void> {
    const publicLobbyId = this.publicLobbyId();
    const session = this.session();

    if (!publicLobbyId || !session?.playerId) {
      return;
    }

    this.articleError.set(null);

    try {
      const lobby = await this.lobbyApi.updateArticle(publicLobbyId, slot, {
        playerId: session.playerId,
        title
      });

      this.applyLobbySnapshot(lobby);

      if (slot === 'start') {
        this.startSuggestions.set([]);
      } else {
        this.targetSuggestions.set([]);
      }
    } catch (error) {
      this.articleError.set(this.readErrorMessage(error, this.localization.t('lobby.articleUpdateError')));
    }
  }

  protected async randomizeArticle(slot: 'start' | 'target'): Promise<void> {
    const publicLobbyId = this.publicLobbyId();
    const session = this.session();

    if (!publicLobbyId || !session?.playerId || !this.isHost()) {
      return;
    }

    this.articleError.set(null);

    if (slot === 'start') {
      this.isRandomizingStart.set(true);
    } else {
      this.isRandomizingTarget.set(true);
    }

    try {
      const lobby = await this.lobbyApi.randomizeArticle(publicLobbyId, slot, {
        playerId: session.playerId
      });

      this.applyLobbySnapshot(lobby);

      if (slot === 'start') {
        this.startSuggestions.set([]);
      } else {
        this.targetSuggestions.set([]);
      }
    } catch (error) {
      this.articleError.set(this.readErrorMessage(error, this.localization.t('lobby.articleUpdateError')));
    } finally {
      if (slot === 'start') {
        this.isRandomizingStart.set(false);
      } else {
        this.isRandomizingTarget.set(false);
      }
    }
  }

  protected async copyShareLink(): Promise<void> {
    if (!this.shareUrl()) {
      return;
    }

    try {
      await navigator.clipboard.writeText(this.shareUrl());
      this.copiedShareLink.set(true);
    } catch {
      this.copiedShareLink.set(false);
    }
  }

  private readErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && error.error) {
      const payload = error.error as ErrorPayload;
      return payload.message ?? fallback;
    }

    return fallback;
  }

  private async searchArticles(slot: 'start' | 'target', query: string): Promise<void> {
    const lobby = this.lobby();

    if (!lobby || query.trim().length < 2) {
      return;
    }

    if (slot === 'start') {
      this.isSearchingStart.set(true);
    } else {
      this.isSearchingTarget.set(true);
    }

    try {
      const suggestions = await this.lobbyApi.searchArticles(lobby.settings.language, query.trim());

      if (slot === 'start') {
        this.startSuggestions.set(suggestions);
      } else {
        this.targetSuggestions.set(suggestions);
      }
    } catch (error) {
      this.articleError.set(this.readErrorMessage(error, this.localization.t('lobby.articleUpdateError')));
    } finally {
      if (slot === 'start') {
        this.isSearchingStart.set(false);
      } else {
        this.isSearchingTarget.set(false);
      }
    }
  }

  private syncSearchInputs(lobby: LobbyStateView): void {
    this.startSearch.set(lobby.settings.startArticle?.displayTitle ?? '');
    this.targetSearch.set(lobby.settings.targetArticle?.displayTitle ?? '');
  }

  private clearTimer(slot: 'start' | 'target'): void {
    const timer = slot === 'start' ? this.startSearchTimer : this.targetSearchTimer;

    if (timer !== null) {
      window.clearTimeout(timer);
    }
  }

  private toArticleSummary(displayTitle?: string | null, selectionMode?: string): string {
    if (!displayTitle) {
      return this.localization.t('lobby.notSelected');
    }

    const mode = selectionMode === 'random'
      ? this.localization.t('lobby.selectionMode.random')
      : this.localization.t('lobby.selectionMode.manual');

    return `${displayTitle} (${mode})`;
  }

  private connectRealtime(publicLobbyId: string, reconnectToken: string): void {
    this.lobbyRealtime.connect(publicLobbyId, reconnectToken, {
      onLobbyChanged: (lobby) => this.applyLobbySnapshot(lobby),
      onError: () => this.realtimeError.set(this.localization.t('lobby.realtimeError'))
    });
  }

  private applyLobbySnapshot(lobby: LobbyStateView): void {
    this.lobby.set(lobby);
    this.selectedLanguage = lobby.settings.language;
    this.localization.setLanguage(lobby.settings.uiLanguage);
    this.syncSearchInputs(lobby);
    this.startCountdownTimer(lobby);

    if (this.joinState() && this.isLobbyInMatch(lobby)) {
      void this.router.navigate(['/match', lobby.publicLobbyId], {
        queryParams: { mode: 'multiplayer' }
      });
    }
  }

  protected isLobbyInMatch(lobby: LobbyStateView): boolean {
    return lobby.status.toLowerCase() === 'inmatch';
  }

  private startCountdownTimer(lobby: LobbyStateView): void {
    this.stopCountdownTimer();

    if (!lobby.countdown) {
      this.countdownSeconds.set(0);
      return;
    }

    const update = () => {
      const remaining = Math.max(0, Math.ceil((Date.parse(lobby.countdown!.endsAtUtc) - Date.now()) / 1000));
      this.countdownSeconds.set(remaining);
    };

    update();

    if (this.countdownSeconds() > 0) {
      this.countdownTimer = window.setInterval(update, 250);
    }
  }

  private stopCountdownTimer(): void {
    if (this.countdownTimer !== null) {
      window.clearInterval(this.countdownTimer);
      this.countdownTimer = null;
    }
  }
}

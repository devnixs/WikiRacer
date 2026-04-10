import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalizationService } from '../../core/localization/localization.service';
import { PageFrameComponent } from '../../shared/layout/page-frame';
import { ChipComponent } from '../../shared/ui/chip';
import { LobbyApiService } from '../lobby/data/lobby-api.service';
import { ErrorPayload, LobbyStateView } from '../lobby/data/lobby.models';
import { LobbyRealtimeService } from '../lobby/data/lobby-realtime.service';
import { LobbySessionService } from '../lobby/data/lobby-session.service';
import { TocEntry, ArticleHtmlSanitizerService } from './article-html-sanitizer.service';
import { ArticleRenderResponse } from './data/article-render.models';
import { ArticleRenderService } from './data/article-render.service';
import { MatchApiService } from './data/match-api.service';
import { MatchPlayerStateView, MatchStateView } from './data/match.models';
import { SoloRaceSnapshot } from './data/solo-race.models';
import { SoloRaceSessionService } from './data/solo-race-session.service';

interface MatchAnnouncement {
  readonly id: string;
  readonly type: 'finish' | 'abandon';
  readonly playerId: string;
  readonly displayName: string;
  readonly placement: number | null;
  readonly hopCount: number;
}

interface PlayerFinishedWsPayload {
  matchId: string;
  playerId: string;
  hopCount: number;
  placement: number;
}

interface PlayerAbandonedWsPayload {
  matchId: string;
  playerId: string;
}

interface MatchSnapshotWsPayload {
  match: MatchStateView;
  isResync: boolean;
}

@Component({
  selector: 'app-match-page',
  imports: [CommonModule, PageFrameComponent, ChipComponent],
  templateUrl: './match-page.html',
  styleUrl: './match-page.scss'
})
export class MatchPageComponent implements OnInit, OnDestroy {
  protected readonly publicLobbyId = signal<string | null>(null);
  protected readonly lobby = signal<LobbyStateView | null>(null);
  protected readonly renderedArticle = signal<ArticleRenderResponse | null>(null);
  protected readonly renderedHtml = signal<SafeHtml | null>(null);
  protected readonly tocEntries = signal<readonly TocEntry[]>([]);
  protected readonly tocExpanded = signal(false);
  protected readonly soloRace = signal<SoloRaceSnapshot | null>(null);
  protected readonly multiplayerMatch = signal<MatchStateView | null>(null);
  protected readonly announcements = signal<readonly MatchAnnouncement[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly localization = inject(LocalizationService);
  protected readonly isMultiplayerMode = signal(false);
  protected readonly elapsedSeconds = signal(0);
  protected readonly elapsedDisplay = computed(() => this.formatElapsed(this.elapsedSeconds()));
  protected readonly canStartSoloRace = computed(() => {
    const lobby = this.lobby();
    return !this.isMultiplayerMode() && !!lobby?.settings.startArticle && !!lobby.settings.targetArticle;
  });
  protected readonly currentPlayerId = computed(() => this.session()?.playerId ?? null);
  protected readonly currentMultiplayerPlayer = computed(() => {
    const match = this.multiplayerMatch();
    const playerId = this.currentPlayerId();
    return match?.players.find((player) => player.playerId === playerId) ?? null;
  });
  protected readonly sourceDisplay = computed(() => this.multiplayerMatch()?.startArticle.displayTitle ?? this.lobby()?.settings.startArticle?.displayTitle ?? this.localization.t('lobby.notSelected'));
  protected readonly targetDisplay = computed(() => this.multiplayerMatch()?.targetArticle.displayTitle ?? this.lobby()?.settings.targetArticle?.displayTitle ?? this.localization.t('lobby.notSelected'));
  protected readonly currentArticleDisplay = computed(() => this.currentMultiplayerPlayer()?.currentArticleTitle ?? this.soloRace()?.currentArticleTitle ?? this.localization.t('lobby.notSelected'));
  protected readonly hopCountDisplay = computed(() => `${this.currentMultiplayerPlayer()?.hopCount ?? this.soloRace()?.hopCount ?? 0}`);
  protected readonly statusDisplay = computed(() => {
    if (this.isMultiplayerMode()) {
      const status = this.currentMultiplayerPlayer()?.status;
      return status ? this.playerStatusLabel(status) : this.localization.t('match.status.active');
    }

    return this.soloRace()?.status === 'finished'
      ? this.localization.t('match.status.finished')
      : this.localization.t('match.status.active');
  });

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly lobbyApi = inject(LobbyApiService);
  private readonly lobbySessionService = inject(LobbySessionService);
  private readonly session = signal<ReturnType<LobbySessionService['get']>>(null);
  private readonly articleRenderService = inject(ArticleRenderService);
  private readonly articleHtmlSanitizer = inject(ArticleHtmlSanitizerService);
  private readonly domSanitizer = inject(DomSanitizer);
  private readonly soloRaceSessionService = inject(SoloRaceSessionService);
  private readonly matchApi = inject(MatchApiService);
  private readonly lobbyRealtime = inject(LobbyRealtimeService);
  private timerId: number | null = null;
  private connectedLobbyId: string | null = null;

  ngOnInit(): void {
    this.route.paramMap.subscribe(() => void this.loadPage());
    this.route.queryParamMap.subscribe(() => void this.loadPage());
  }

  ngOnDestroy(): void {
    this.stopTimer();
    this.connectedLobbyId = null;
    this.lobbyRealtime.disconnect();
  }

  protected dismissAnnouncement(id: string): void {
    this.announcements.update((list) => list.filter((a) => a.id !== id));
  }

  protected toggleToc(): void {
    this.tocExpanded.update((v) => !v);
  }

  protected onTocClick(event: MouseEvent, id: string): void {
    event.preventDefault();
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' });
    this.tocExpanded.set(false);
  }

  protected playerStatusLabel(status: string): string {
    switch (status) {
      case 'active': return this.localization.t('match.status.active');
      case 'finished': return this.localization.t('match.status.finished');
      case 'abandoned': return this.localization.t('match.status.abandoned');
      case 'disconnected': return this.localization.t('match.status.disconnected');
      default: return status;
    }
  }

  protected async onArticleSurfaceClick(event: MouseEvent): Promise<void> {
    const target = event.target;

    if (!(target instanceof Element)) {
      return;
    }

    const anchor = target.closest('a[data-internal-link]');

    if (!(anchor instanceof HTMLAnchorElement)) {
      return;
    }

    event.preventDefault();

    const publicLobbyId = this.publicLobbyId();
    const articleTitle = anchor.getAttribute('data-internal-link');

    if (!publicLobbyId || !articleTitle) {
      return;
    }

    await this.router.navigate(['/match', publicLobbyId], {
      queryParams: {
        article: articleTitle,
        mode: this.isMultiplayerMode() ? 'multiplayer' : null
      }
    });
  }

  protected async startSoloRace(): Promise<void> {
    const lobby = this.lobby();
    const publicLobbyId = this.publicLobbyId();

    if (!lobby?.settings.startArticle || !lobby.settings.targetArticle || !publicLobbyId) {
      return;
    }

    const snapshot = this.soloRaceSessionService.start(
      publicLobbyId,
      lobby.settings.language,
      lobby.settings.startArticle,
      lobby.settings.targetArticle
    );

    this.soloRace.set(snapshot);
    this.restartSoloTimer(snapshot);

    await this.router.navigate(['/match', publicLobbyId], {
      queryParams: { article: lobby.settings.startArticle.title, startSolo: null, mode: null },
      queryParamsHandling: 'merge'
    });
  }

  protected async openResults(): Promise<void> {
    const publicLobbyId = this.publicLobbyId();

    if (!publicLobbyId) {
      return;
    }

    await this.router.navigate(['/results'], {
      queryParams: { lobby: publicLobbyId }
    });
  }

  protected async abandonMatch(): Promise<void> {
    const match = this.multiplayerMatch();
    const playerId = this.currentPlayerId();

    if (!match || !playerId) {
      return;
    }

    const updated = await this.matchApi.abandon(match.matchId, { playerId });
    this.multiplayerMatch.set(updated);
    this.restartMultiplayerTimer(updated);
  }

  private handleWsMessage(messageType: string, payload: unknown): void {
    if (messageType === 'match.snapshot') {
      const event = payload as MatchSnapshotWsPayload;
      this.multiplayerMatch.set(event.match);
      this.restartMultiplayerTimer(event.match);
      return;
    }

    if (messageType === 'match.player.finished') {
      const event = payload as PlayerFinishedWsPayload;
      const match = this.applyPlayerFinishedEvent(event);
      const player = match?.players.find((p) => p.playerId === event.playerId);

      if (player) {
        this.addAnnouncement({
          id: `finish-${event.playerId}-${event.placement}`,
          type: 'finish',
          playerId: event.playerId,
          displayName: player.displayName,
          placement: event.placement,
          hopCount: event.hopCount
        });
      }

      return;
    }

    if (messageType === 'match.player.abandoned') {
      const event = payload as PlayerAbandonedWsPayload;
      const match = this.applyPlayerAbandonedEvent(event);
      const player = match?.players.find((p) => p.playerId === event.playerId);

      if (player) {
        this.addAnnouncement({
          id: `abandon-${event.playerId}`,
          type: 'abandon',
          playerId: event.playerId,
          displayName: player.displayName,
          placement: null,
          hopCount: player.hopCount
        });
      }
    }
  }

  private addAnnouncement(announcement: MatchAnnouncement): void {
    this.announcements.update((list) => {
      if (list.some((a) => a.id === announcement.id)) {
        return list;
      }

      return [...list, announcement];
    });
  }

  private applyPlayerFinishedEvent(event: PlayerFinishedWsPayload): MatchStateView | null {
    const match = this.multiplayerMatch();

    if (!match || match.matchId !== event.matchId) {
      return match;
    }

    const updated: MatchStateView = {
      ...match,
      players: match.players.map((player) => player.playerId === event.playerId
        ? {
            ...player,
            status: 'finished',
            currentArticleTitle: match.targetArticle.title,
            hopCount: event.hopCount,
            placement: event.placement
          }
        : player)
    };

    this.multiplayerMatch.set(updated);
    return updated;
  }

  private applyPlayerAbandonedEvent(event: PlayerAbandonedWsPayload): MatchStateView | null {
    const match = this.multiplayerMatch();

    if (!match || match.matchId !== event.matchId) {
      return match;
    }

    const updated: MatchStateView = {
      ...match,
      players: match.players.map((player) => player.playerId === event.playerId
        ? { ...player, status: 'abandoned' }
        : player)
    };

    this.multiplayerMatch.set(updated);
    return updated;
  }

  private async loadPage(): Promise<void> {
    const publicLobbyId = this.route.snapshot.paramMap.get('publicLobbyId');
    this.publicLobbyId.set(publicLobbyId);
    this.errorMessage.set(null);

    if (!publicLobbyId) {
      this.lobby.set(null);
      this.renderedArticle.set(null);
      this.renderedHtml.set(null);
      this.tocEntries.set([]);
      this.soloRace.set(null);
      this.multiplayerMatch.set(null);
      this.stopTimer();
      this.isLoading.set(false);
      return;
    }

    this.isLoading.set(true);
    this.session.set(this.lobbySessionService.get(publicLobbyId));

    try {
      const lobby = await this.lobbyApi.getLobby(publicLobbyId);
      this.lobby.set(lobby);
      this.localization.setLanguage(lobby.settings.uiLanguage);

      const requestedMode = this.route.snapshot.queryParamMap.get('mode');
      const isMultiplayer = requestedMode === 'multiplayer' || lobby.status.toLowerCase() === 'inmatch';
      this.isMultiplayerMode.set(isMultiplayer);

      if (isMultiplayer) {
        const session = this.session();

        if (session?.reconnectToken && this.connectedLobbyId !== publicLobbyId) {
          this.connectedLobbyId = publicLobbyId;
          this.lobbyRealtime.connect(publicLobbyId, session.reconnectToken, {
            onMessage: (type, payload) => this.handleWsMessage(type, payload),
            onError: () => { /* non-fatal: REST still provides state */ }
          });
        }

        const match = await this.matchApi.getByLobby(publicLobbyId);
        this.multiplayerMatch.set(match);
        this.soloRace.set(null);
        this.restartMultiplayerTimer(match);

        const requestedTitle = this.route.snapshot.queryParamMap.get('article')?.trim()
          || this.currentMultiplayerPlayer()?.currentArticleTitle
          || match.startArticle.title;

        await this.renderArticle(lobby.settings.language, requestedTitle, publicLobbyId);
        return;
      }

      const startSolo = this.route.snapshot.queryParamMap.get('startSolo') === '1';
      let soloRace = this.soloRaceSessionService.get(publicLobbyId);

      if (startSolo && lobby.settings.startArticle && lobby.settings.targetArticle) {
        soloRace = this.soloRaceSessionService.start(
          publicLobbyId,
          lobby.settings.language,
          lobby.settings.startArticle,
          lobby.settings.targetArticle
        );
      }

      if (soloRace && !this.isCompatibleSoloRace(soloRace, lobby)) {
        this.soloRaceSessionService.clear(publicLobbyId);
        soloRace = null;
      }

      this.soloRace.set(soloRace);
      this.multiplayerMatch.set(null);
      this.restartSoloTimer(soloRace);

      const requestedTitle = this.route.snapshot.queryParamMap.get('article')?.trim()
        || soloRace?.currentArticleTitle
        || lobby.settings.startArticle?.title
        || null;

      if (!requestedTitle) {
        this.renderedArticle.set(null);
        this.renderedHtml.set(null);
        this.tocEntries.set([]);
        return;
      }

      await this.renderArticle(lobby.settings.language, requestedTitle, publicLobbyId);
    } catch (error) {
      this.renderedArticle.set(null);
      this.renderedHtml.set(this.domSanitizer.bypassSecurityTrustHtml(''));
      this.tocEntries.set([]);
      this.errorMessage.set(this.readErrorMessage(error, this.localization.t('match.error')));
    } finally {
      this.isLoading.set(false);
    }
  }

  private async renderArticle(language: string, requestedTitle: string, publicLobbyId: string): Promise<void> {
    const article = await this.articleRenderService.render(language, requestedTitle);
    const prepared = this.articleHtmlSanitizer.prepare(article, publicLobbyId);

    this.renderedArticle.set(article);
    this.renderedHtml.set(prepared.html);
    this.tocEntries.set(prepared.toc);

    if (this.isMultiplayerMode()) {
      await this.reportMultiplayerProgress(article.title);
    } else {
      this.reportSoloProgress(article.title);
    }

    if (this.route.snapshot.queryParamMap.get('article') !== article.title) {
      await this.router.navigate(['/match', publicLobbyId], {
        queryParams: { article: article.title, mode: this.isMultiplayerMode() ? 'multiplayer' : null, startSolo: null },
        replaceUrl: true
      });
    }
  }

  private async reportMultiplayerProgress(canonicalTitle: string): Promise<void> {
    const match = this.multiplayerMatch();
    const player = this.currentMultiplayerPlayer();

    if (!match || !player || player.status !== 'active' || player.currentArticleTitle === canonicalTitle) {
      return;
    }

    const updated = await this.matchApi.reportProgress(match.matchId, {
      playerId: player.playerId,
      currentArticleTitle: canonicalTitle,
      reportedAtUtc: new Date().toISOString()
    });

    this.multiplayerMatch.set(updated);
    this.restartMultiplayerTimer(updated);
  }

  private reportSoloProgress(canonicalTitle: string): void {
    const snapshot = this.soloRace();

    if (!snapshot || snapshot.status === 'finished' || snapshot.currentArticleTitle === canonicalTitle) {
      return;
    }

    const updated: SoloRaceSnapshot = {
      ...snapshot,
      currentArticleTitle: canonicalTitle,
      hopCount: snapshot.hopCount + 1,
      visitedTitles: [...snapshot.visitedTitles, canonicalTitle]
    };

    if (canonicalTitle === snapshot.targetArticle.title) {
      updated.status = 'finished';
      updated.finishedAtUtc = new Date().toISOString();
    }

    this.soloRace.set(updated);
    this.soloRaceSessionService.save(updated);
    this.restartSoloTimer(updated);
  }

  private isCompatibleSoloRace(snapshot: SoloRaceSnapshot, lobby: LobbyStateView): boolean {
    return snapshot.language === lobby.settings.language
      && snapshot.startArticle.title === lobby.settings.startArticle?.title
      && snapshot.targetArticle.title === lobby.settings.targetArticle?.title;
  }

  private restartSoloTimer(snapshot: SoloRaceSnapshot | null): void {
    this.stopTimer();

    if (!snapshot) {
      this.elapsedSeconds.set(0);
      return;
    }

    const update = () => {
      const startedAt = Date.parse(snapshot.startedAtUtc);
      const end = snapshot.finishedAtUtc ? Date.parse(snapshot.finishedAtUtc) : Date.now();
      this.elapsedSeconds.set(Math.max(0, Math.floor((end - startedAt) / 1000)));
    };

    update();

    if (snapshot.status === 'active') {
      this.timerId = window.setInterval(update, 1000);
    }
  }

  private restartMultiplayerTimer(match: MatchStateView | null): void {
    this.stopTimer();

    if (!match) {
      this.elapsedSeconds.set(0);
      return;
    }

    const update = () => {
      const startedAt = Date.parse(match.startedAtUtc);
      const end = Date.now();
      this.elapsedSeconds.set(Math.max(0, Math.floor((end - startedAt) / 1000)));
    };

    update();

    if (match.status === 'inProgress') {
      this.timerId = window.setInterval(update, 1000);
    }
  }

  private stopTimer(): void {
    if (this.timerId !== null) {
      window.clearInterval(this.timerId);
      this.timerId = null;
    }
  }

  private readErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && error.error) {
      const payload = error.error as ErrorPayload;
      return payload.message ?? fallback;
    }

    return fallback;
  }

  private formatElapsed(totalSeconds: number): string {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  }
}

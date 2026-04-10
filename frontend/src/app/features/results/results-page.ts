import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LocalizationService } from '../../core/localization/localization.service';
import { PageFrameComponent } from '../../shared/layout/page-frame';
import { FeatureCardComponent } from '../../shared/ui/feature-card';
import { MetricTileComponent } from '../../shared/ui/metric-tile';
import { MatchApiService } from '../match/data/match-api.service';
import { MatchPlayerStateView, MatchStateView } from '../match/data/match.models';
import { SoloRaceSessionService } from '../match/data/solo-race-session.service';

@Component({
  selector: 'app-results-page',
  templateUrl: './results-page.html',
  imports: [CommonModule, PageFrameComponent, FeatureCardComponent, MetricTileComponent],
  styleUrl: './results-page.scss'
})
export class ResultsPageComponent implements OnInit {
  protected readonly localization = inject(LocalizationService);
  protected readonly isLoading = signal(true);
  protected readonly soloResult = signal<ReturnType<SoloRaceSessionService['get']>>(null);
  protected readonly multiplayerResult = signal<MatchStateView | null>(null);
  protected readonly pageTitle = computed(() => this.multiplayerResult()
    ? this.localization.t('results.multiplayerTitle')
    : this.localization.t('results.title'));
  protected readonly pageDescription = computed(() => this.multiplayerResult()
    ? this.localization.t('results.multiplayerDescription')
    : this.localization.t('results.description'));
  protected readonly soloHopCountDisplay = computed(() => `${this.soloResult()?.hopCount ?? 0}`);
  protected readonly soloElapsedDisplay = computed(() => {
    const result = this.soloResult();

    if (!result) {
      return '00:00';
    }

    return this.formatDuration(result.startedAtUtc, result.finishedAtUtc ?? result.startedAtUtc);
  });
  protected readonly winnerDisplay = computed(() => this.rankedPlayers().find((player) => player.status === 'finished')?.displayName ?? '—');
  protected readonly finishedCountDisplay = computed(() => `${this.multiplayerResult()?.players.filter((player) => player.status === 'finished').length ?? 0}`);
  protected readonly abandonedCountDisplay = computed(() => `${this.multiplayerResult()?.players.filter((player) => player.status === 'abandoned').length ?? 0}`);
  protected readonly rankedPlayers = computed(() => {
    const players = this.multiplayerResult()?.players ?? [];

    return [...players].sort((left, right) => {
      const leftPlacement = left.placement ?? Number.MAX_SAFE_INTEGER;
      const rightPlacement = right.placement ?? Number.MAX_SAFE_INTEGER;

      if (leftPlacement !== rightPlacement) {
        return leftPlacement - rightPlacement;
      }

      return this.statusRank(left.status) - this.statusRank(right.status)
        || left.displayName.localeCompare(right.displayName);
    });
  });

  private readonly route = inject(ActivatedRoute);
  private readonly soloRaceSessionService = inject(SoloRaceSessionService);
  private readonly matchApi = inject(MatchApiService);

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      void this.loadResults(params.get('lobby'));
    });
  }

  protected placementLabel(player: MatchPlayerStateView): string {
    const placement = player.placement ?? '—';
    return `${placement}. ${player.displayName}`;
  }

  protected finishTimeLabel(player: MatchPlayerStateView): string {
    if (!player.finishTime) {
      return this.localization.t('results.stillRacing');
    }

    return this.formatRecordedDuration(player.finishTime);
  }

  protected playerStatusLabel(status: MatchPlayerStateView['status']): string {
    switch (status) {
      case 'finished':
        return this.localization.t('match.status.finished');
      case 'abandoned':
        return this.localization.t('match.status.abandoned');
      case 'disconnected':
        return this.localization.t('match.status.disconnected');
      default:
        return this.localization.t('match.status.active');
    }
  }

  private async loadResults(publicLobbyId: string | null): Promise<void> {
    this.isLoading.set(true);
    this.soloResult.set(null);
    this.multiplayerResult.set(null);

    if (!publicLobbyId) {
      this.isLoading.set(false);
      return;
    }

    try {
      const match = await this.matchApi.getByLobby(publicLobbyId);

      if (match.status === 'finished') {
        this.localization.setLanguage(match.language);
        this.multiplayerResult.set(match);
        this.isLoading.set(false);
        return;
      }
    } catch {
      // Fall back to the local solo snapshot when no multiplayer match exists.
    }

    const result = this.soloRaceSessionService.get(publicLobbyId);

    if (result?.status === 'finished') {
      this.localization.setLanguage(result.language);
      this.soloResult.set(result);
    }

    this.isLoading.set(false);
  }

  private formatDuration(startedAtUtc: string, endedAtUtc: string): string {
    const totalSeconds = Math.max(0, Math.floor((Date.parse(endedAtUtc) - Date.parse(startedAtUtc)) / 1000));
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;

    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  }

  private formatRecordedDuration(value: string): string {
    if (value.includes('T')) {
      return this.formatDuration(this.multiplayerResult()?.startedAtUtc ?? value, value);
    }

    const [hoursPart = '0', minutesPart = '0', secondsPart = '0'] = value.split(':');
    const hours = Number.parseInt(hoursPart, 10) || 0;
    const minutes = Number.parseInt(minutesPart, 10) || 0;
    const seconds = Math.floor(Number.parseFloat(secondsPart) || 0);
    const totalMinutes = hours * 60 + minutes;

    return `${String(totalMinutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  }

  private statusRank(status: MatchPlayerStateView['status']): number {
    switch (status) {
      case 'finished':
        return 0;
      case 'active':
        return 1;
      case 'disconnected':
        return 2;
      case 'abandoned':
        return 3;
      default:
        return 4;
    }
  }
}

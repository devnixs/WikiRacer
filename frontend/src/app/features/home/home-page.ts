import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PageFrameComponent } from '../../shared/layout/page-frame';
import { FeatureCardComponent } from '../../shared/ui/feature-card';
import { MetricTileComponent } from '../../shared/ui/metric-tile';
import { ChipComponent } from '../../shared/ui/chip';
import { LobbyApiService } from '../lobby/data/lobby-api.service';
import { LobbySessionService } from '../lobby/data/lobby-session.service';
import { LocalizationService } from '../../core/localization/localization.service';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.html',
  imports: [PageFrameComponent, FeatureCardComponent, MetricTileComponent, ChipComponent, FormsModule],
  styleUrl: './home-page.scss'
})
export class HomePageComponent {
  protected displayName = '';
  protected readonly isCreating = signal(false);
  protected readonly createError = signal<string | null>(null);

  private readonly lobbyApi = inject(LobbyApiService);
  private readonly lobbySessionService = inject(LobbySessionService);
  private readonly router = inject(Router);
  protected readonly localization = inject(LocalizationService);

  protected async createLobby(): Promise<void> {
    if (!this.displayName.trim() || this.isCreating()) {
      return;
    }

    this.isCreating.set(true);
    this.createError.set(null);

    try {
      const response = await this.lobbyApi.createLobby({
        displayName: this.displayName.trim(),
        language: 'fr',
        uiLanguage: 'fr',
        playerCap: 8,
        timeLimitSeconds: 600
      });

      this.lobbySessionService.save({
        publicLobbyId: response.lobby.publicLobbyId,
        playerId: response.playerId,
        displayName: this.displayName.trim(),
        reconnectToken: response.reconnectToken
      });

      this.localization.setLanguage(response.lobby.settings.uiLanguage);

      await this.router.navigate(['/lobby', response.lobby.publicLobbyId]);
    } catch {
      this.createError.set(this.localization.t('home.create.error'));
    } finally {
      this.isCreating.set(false);
    }
  }
}

import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LocalizationService } from './core/localization/localization.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly localization = inject(LocalizationService);

  protected readonly navigation = [
    { key: 'nav.home', path: '/' },
    { key: 'nav.lobby', path: '/lobby' },
    { key: 'nav.match', path: '/match' },
    { key: 'nav.results', path: '/results' }
  ] as const;
}

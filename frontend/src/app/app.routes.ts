import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/home-page';
import { LobbyPageComponent } from './features/lobby/lobby-page';
import { MatchPageComponent } from './features/match/match-page';
import { ResultsPageComponent } from './features/results/results-page';

export const routes: Routes = [
  { path: '', component: HomePageComponent, title: 'WikiRacer' },
  { path: 'lobby', component: LobbyPageComponent, title: 'WikiRacer | Lobby' },
  { path: 'lobby/:publicLobbyId', component: LobbyPageComponent, title: 'WikiRacer | Lobby' },
  { path: 'match', component: MatchPageComponent, title: 'WikiRacer | Match' },
  { path: 'match/:publicLobbyId', component: MatchPageComponent, title: 'WikiRacer | Match' },
  { path: 'results', component: ResultsPageComponent, title: 'WikiRacer | Results' },
  { path: '**', redirectTo: '' }
];

import { Injectable, computed, signal } from '@angular/core';

type Locale = 'en' | 'fr';

type TranslationKey =
  | 'nav.home'
  | 'nav.lobby'
  | 'nav.match'
  | 'nav.results'
  | 'app.kicker'
  | 'app.strap'
  | 'home.eyebrow'
  | 'home.title'
  | 'home.description'
  | 'home.create'
  | 'home.create.button'
  | 'home.create.creating'
  | 'home.create.placeholder'
  | 'home.create.error'
  | 'lobby.eyebrow'
  | 'lobby.title'
  | 'lobby.description'
  | 'lobby.loading'
  | 'lobby.noSelection'
  | 'lobby.noSelection.body'
  | 'lobby.back'
  | 'lobby.share'
  | 'lobby.copy'
  | 'lobby.copied'
  | 'lobby.joined'
  | 'lobby.joinForm'
  | 'lobby.joinButton'
  | 'lobby.joining'
  | 'lobby.members'
  | 'lobby.joinState.open'
  | 'lobby.joinState.full'
  | 'lobby.language'
  | 'lobby.players'
  | 'lobby.code'
  | 'lobby.hostLanguage'
  | 'lobby.hostLanguageHint'
  | 'lobby.changeLanguage'
  | 'lobby.changeLanguagePending'
  | 'lobby.changeLanguageError'
  | 'lobby.searchStart'
  | 'lobby.searchTarget'
  | 'lobby.searchPlaceholder'
  | 'lobby.searching'
  | 'lobby.noSuggestions'
  | 'lobby.pickSource'
  | 'lobby.pickTarget'
  | 'lobby.randomizeSource'
  | 'lobby.randomizeTarget'
  | 'lobby.randomizing'
  | 'lobby.selectionMode.manual'
  | 'lobby.selectionMode.random'
  | 'lobby.articleUpdateError'
  | 'lobby.selectedArticles'
  | 'lobby.source'
  | 'lobby.target'
  | 'lobby.notSelected'
  | 'lobby.openViewer'
  | 'lobby.startSolo'
  | 'lobby.ready'
  | 'lobby.notReady'
  | 'lobby.countdown'
  | 'lobby.countdownWaiting'
  | 'lobby.realtimeError'
  | 'lobby.startMultiplayer'
  | 'lobby.openMultiplayer'
  | 'match.eyebrow'
  | 'match.title'
  | 'match.description'
  | 'match.loading'
  | 'match.error'
  | 'match.openFromLobby'
  | 'match.source'
  | 'match.target'
  | 'match.sourceLink'
  | 'match.startRace'
  | 'match.restartRace'
  | 'match.elapsed'
  | 'match.hops'
  | 'match.currentArticle'
  | 'match.status.active'
  | 'match.status.finished'
  | 'match.finishBanner'
  | 'match.viewResults'
  | 'match.players'
  | 'match.multiplayerStartError'
  | 'match.multiplayerState'
  | 'match.abandon'
  | 'match.viewer.empty'
  | 'match.viewer.attribution'
  | 'match.viewer.externalNotice'
  | 'match.toc'
  | 'match.raceStatus'
  | 'match.player.you'
  | 'match.status.abandoned'
  | 'match.status.disconnected'
  | 'match.reachedTarget'
  | 'match.leftTheRace'
  | 'match.youFinished'
  | 'match.place'
  | 'match.dismiss'
  | 'match.matchComplete'
  | 'results.eyebrow'
  | 'results.title'
  | 'results.description'
  | 'results.multiplayerTitle'
  | 'results.multiplayerDescription'
  | 'results.elapsed'
  | 'results.hops'
  | 'results.currentArticle'
  | 'results.noRace'
  | 'results.winner'
  | 'results.playersFinished'
  | 'results.playersAbandoned'
  | 'results.route'
  | 'results.state'
  | 'results.finalRanking'
  | 'results.finishTime'
  | 'results.status'
  | 'results.stillRacing'
  | 'language.fr'
  | 'language.en';

const translations: Record<Locale, Record<TranslationKey, string>> = {
  en: {
    'nav.home': 'Home',
    'nav.lobby': 'Lobby',
    'nav.match': 'Match',
    'nav.results': 'Results',
    'app.kicker': 'WikiRacer',
    'app.strap': 'Multiplayer Wikipedia racing, structured from day one for real-time gameplay and long-term maintainability.',
    'home.eyebrow': 'Home',
    'home.title': 'A vivid shell for realtime Wikipedia racing.',
    'home.description': 'The frontend foundation now has reusable layout primitives, shared UI blocks, and route-level feature slices ready for the next stories.',
    'home.create': 'Start a lobby',
    'home.create.button': 'Create Lobby',
    'home.create.creating': 'Creating...',
    'home.create.placeholder': 'Your temporary display name',
    'home.create.error': 'The lobby could not be created right now.',
    'lobby.eyebrow': 'Lobby',
    'lobby.title': 'Shareable rooms and direct-URL join flows are live.',
    'lobby.description': 'This page can resolve a lobby from its public URL, auto-join when a local runtime session exists, and fall back to a manual join form when it does not.',
    'lobby.loading': 'Loading lobby',
    'lobby.noSelection': 'No active lobby selected',
    'lobby.noSelection.body': 'Create a lobby from the home page to get a canonical share URL, then reopen that route to rejoin from the same runtime session.',
    'lobby.back': 'Return home',
    'lobby.share': 'Canonical Invite URL',
    'lobby.copy': 'Copy Link',
    'lobby.copied': 'Copied',
    'lobby.joined': 'Joined as',
    'lobby.joinForm': 'Join this lobby',
    'lobby.joinButton': 'Join Lobby',
    'lobby.joining': 'Joining...',
    'lobby.members': 'Current participants',
    'lobby.joinState.open': 'Lobby accepting players',
    'lobby.joinState.full': 'Lobby full',
    'lobby.language': 'Language',
    'lobby.players': 'Players',
    'lobby.code': 'Lobby Code',
    'lobby.hostLanguage': 'Host language control',
    'lobby.hostLanguageHint': 'The host chooses the game language once, and the UI follows the same selection in version 1.',
    'lobby.changeLanguage': 'Apply Language',
    'lobby.changeLanguagePending': 'Updating...',
    'lobby.changeLanguageError': 'The lobby language could not be updated.',
    'lobby.searchStart': 'Search source page',
    'lobby.searchTarget': 'Search target page',
    'lobby.searchPlaceholder': 'Search Wikipedia titles',
    'lobby.searching': 'Searching...',
    'lobby.noSuggestions': 'No playable article suggestions found.',
    'lobby.pickSource': 'Use as source',
    'lobby.pickTarget': 'Use as target',
    'lobby.randomizeSource': 'Randomize source',
    'lobby.randomizeTarget': 'Randomize target',
    'lobby.randomizing': 'Randomizing...',
    'lobby.selectionMode.manual': 'manual',
    'lobby.selectionMode.random': 'random',
    'lobby.articleUpdateError': 'The article selection could not be saved.',
    'lobby.selectedArticles': 'Selected articles',
    'lobby.source': 'Source',
    'lobby.target': 'Target',
    'lobby.notSelected': 'Not selected',
    'lobby.openViewer': 'Open article viewer',
    'lobby.startSolo': 'Start solo race',
    'lobby.ready': 'Ready',
    'lobby.notReady': 'Not ready',
    'lobby.countdown': 'Countdown',
    'lobby.countdownWaiting': 'Waiting for all players to be ready.',
    'lobby.realtimeError': 'Realtime lobby sync is temporarily unavailable.',
    'lobby.startMultiplayer': 'Start multiplayer match',
    'lobby.openMultiplayer': 'Open multiplayer match',
    'match.eyebrow': 'Match',
    'match.title': 'Article, contents, and race status in one view.',
    'match.description': 'The match screen shows the Wikipedia article in the reading area, a table of contents in the left panel, and live race status with timer, hop counts, and player states on the right.',
    'match.loading': 'Loading article view...',
    'match.error': 'The article view could not be loaded.',
    'match.openFromLobby': 'Open a lobby with a selected source article to start the in-app viewer.',
    'match.source': 'Source article',
    'match.target': 'Target article',
    'match.sourceLink': 'Original Wikipedia page',
    'match.startRace': 'Start solo race',
    'match.restartRace': 'Restart solo race',
    'match.elapsed': 'Elapsed',
    'match.hops': 'Hops',
    'match.currentArticle': 'Current page',
    'match.status.active': 'Active',
    'match.status.finished': 'Finished',
    'match.finishBanner': 'Destination reached. The solo race is finished.',
    'match.viewResults': 'View results',
    'match.players': 'Players',
    'match.multiplayerStartError': 'The multiplayer match could not be started.',
    'match.multiplayerState': 'Multiplayer state',
    'match.abandon': 'Abandon match',
    'match.viewer.empty': 'No source article is selected for this lobby yet.',
    'match.viewer.attribution': 'Content sourced from Wikipedia. Internal article links stay inside the app; external links open safely in a new tab.',
    'match.viewer.externalNotice': 'External links open in a new tab.',
    'match.toc': 'Contents',
    'match.raceStatus': 'Race status',
    'match.player.you': 'you',
    'match.status.abandoned': 'Abandoned',
    'match.status.disconnected': 'Disconnected',
    'match.reachedTarget': 'reached the target',
    'match.leftTheRace': 'left the race',
    'match.youFinished': 'You reached the target!',
    'match.place': 'Place',
    'match.dismiss': 'Dismiss',
    'match.matchComplete': 'Match complete',
    'results.eyebrow': 'Results',
    'results.title': 'The solo race summary is now driven by recorded match state.',
    'results.description': 'Elapsed time, hop count, and the final reported page are carried from the active match loop into a compact recap surface.',
    'results.multiplayerTitle': 'Multiplayer results now reflect the recorded match standings.',
    'results.multiplayerDescription': 'Placements, finish times, and abandon states are read back from the in-memory match snapshot so the lobby can end on a real shared recap.',
    'results.elapsed': 'Elapsed',
    'results.hops': 'Hops',
    'results.currentArticle': 'Final page',
    'results.noRace': 'No completed solo race is available yet.',
    'results.winner': 'Winner',
    'results.playersFinished': 'Players finished',
    'results.playersAbandoned': 'Players abandoned',
    'results.route': 'Route',
    'results.state': 'State',
    'results.finalRanking': 'Final ranking',
    'results.finishTime': 'Finish time',
    'results.status': 'Status',
    'results.stillRacing': 'Still racing',
    'language.fr': 'French',
    'language.en': 'English'
  },
  fr: {
    'nav.home': 'Accueil',
    'nav.lobby': 'Salon',
    'nav.match': 'Course',
    'nav.results': 'Resultats',
    'app.kicker': 'WikiRacer',
    'app.strap': 'Une course Wikipedia multijoueur, structuree des le premier jour pour le temps reel et la maintenabilite.',
    'home.eyebrow': 'Accueil',
    'home.title': 'Une base visuelle nette pour des courses Wikipedia en temps reel.',
    'home.description': 'La fondation frontend contient maintenant des primitives de mise en page reutilisables, des blocs UI partages et des slices de fonctionnalites prêtes pour les prochaines stories.',
    'home.create': 'Creer un salon',
    'home.create.button': 'Creer le salon',
    'home.create.creating': 'Creation...',
    'home.create.placeholder': 'Votre nom temporaire',
    'home.create.error': 'Le salon ne peut pas etre cree pour le moment.',
    'lobby.eyebrow': 'Salon',
    'lobby.title': 'Les salons partageables et l’entree directe par URL sont en place.',
    'lobby.description': 'Cette page peut resoudre un salon a partir de son URL publique, rejoindre automatiquement avec une session locale existante, puis revenir a un formulaire manuel si necessaire.',
    'lobby.loading': 'Chargement du salon',
    'lobby.noSelection': 'Aucun salon actif selectionne',
    'lobby.noSelection.body': 'Creez un salon depuis la page d’accueil pour obtenir une URL canonique, puis rouvrez cette route pour rejoindre avec la meme session locale.',
    'lobby.back': 'Retour a l’accueil',
    'lobby.share': 'URL d’invitation canonique',
    'lobby.copy': 'Copier le lien',
    'lobby.copied': 'Copie',
    'lobby.joined': 'Connecte en tant que',
    'lobby.joinForm': 'Rejoindre ce salon',
    'lobby.joinButton': 'Rejoindre',
    'lobby.joining': 'Connexion...',
    'lobby.members': 'Participants actuels',
    'lobby.joinState.open': 'Salon ouvert aux joueurs',
    'lobby.joinState.full': 'Salon complet',
    'lobby.language': 'Langue',
    'lobby.players': 'Joueurs',
    'lobby.code': 'Code du salon',
    'lobby.hostLanguage': 'Controle de langue de l’hote',
    'lobby.hostLanguageHint': 'L’hote choisit une seule langue de jeu, et l’interface suit ce meme choix dans la version 1.',
    'lobby.changeLanguage': 'Appliquer la langue',
    'lobby.changeLanguagePending': 'Mise a jour...',
    'lobby.changeLanguageError': 'La langue du salon n’a pas pu etre mise a jour.',
    'lobby.searchStart': 'Rechercher la page source',
    'lobby.searchTarget': 'Rechercher la page cible',
    'lobby.searchPlaceholder': 'Rechercher des titres Wikipedia',
    'lobby.searching': 'Recherche...',
    'lobby.noSuggestions': 'Aucune suggestion de page jouable.',
    'lobby.pickSource': 'Utiliser comme source',
    'lobby.pickTarget': 'Utiliser comme cible',
    'lobby.randomizeSource': 'Tirer une source',
    'lobby.randomizeTarget': 'Tirer une cible',
    'lobby.randomizing': 'Tirage...',
    'lobby.selectionMode.manual': 'manuel',
    'lobby.selectionMode.random': 'aleatoire',
    'lobby.articleUpdateError': 'La selection d’article n’a pas pu etre enregistree.',
    'lobby.selectedArticles': 'Articles selectionnes',
    'lobby.source': 'Source',
    'lobby.target': 'Cible',
    'lobby.notSelected': 'Non selectionne',
    'lobby.openViewer': 'Ouvrir le lecteur d’articles',
    'lobby.startSolo': 'Demarrer une course solo',
    'lobby.ready': 'Pret',
    'lobby.notReady': 'Pas pret',
    'lobby.countdown': 'Compte a rebours',
    'lobby.countdownWaiting': 'En attente que tous les joueurs soient prets.',
    'lobby.realtimeError': 'La synchronisation temps reel du salon est temporairement indisponible.',
    'lobby.startMultiplayer': 'Demarrer la partie multijoueur',
    'lobby.openMultiplayer': 'Ouvrir la partie multijoueur',
    'match.eyebrow': 'Course',
    'match.title': 'Article, sommaire et etat de course dans une seule vue.',
    'match.description': 'L’ecran de course affiche l’article Wikipedia dans la zone de lecture, un sommaire dans le panneau gauche, et l’etat de course en direct avec le chronometre, le nombre de sauts et le statut des joueurs a droite.',
    'match.loading': 'Chargement de l’article...',
    'match.error': 'Le lecteur d’article n’a pas pu etre charge.',
    'match.openFromLobby': 'Ouvrez un salon avec un article source selectionne pour lancer le lecteur integre.',
    'match.source': 'Article source',
    'match.target': 'Article cible',
    'match.sourceLink': 'Page Wikipedia originale',
    'match.startRace': 'Demarrer la course solo',
    'match.restartRace': 'Relancer la course solo',
    'match.elapsed': 'Temps',
    'match.hops': 'Sauts',
    'match.currentArticle': 'Page courante',
    'match.status.active': 'Active',
    'match.status.finished': 'Terminee',
    'match.finishBanner': 'Destination atteinte. La course solo est terminee.',
    'match.viewResults': 'Voir les resultats',
    'match.players': 'Joueurs',
    'match.multiplayerStartError': 'La partie multijoueur n’a pas pu etre demarree.',
    'match.multiplayerState': 'Etat multijoueur',
    'match.abandon': 'Abandonner la partie',
    'match.viewer.empty': 'Aucun article source n’est encore selectionne pour ce salon.',
    'match.viewer.attribution': 'Le contenu provient de Wikipedia. Les liens internes restent dans l’application et les liens externes s’ouvrent de facon sure dans un nouvel onglet.',
    'match.viewer.externalNotice': 'Les liens externes s’ouvrent dans un nouvel onglet.',
    'match.toc': 'Sommaire',
    'match.raceStatus': 'Etat de la course',
    'match.player.you': 'vous',
    'match.status.abandoned': 'Abandonne',
    'match.status.disconnected': 'Deconnecte',
    'match.reachedTarget': 'a atteint la cible',
    'match.leftTheRace': 'a abandonne la course',
    'match.youFinished': 'Vous avez atteint la cible!',
    'match.place': 'Rang',
    'match.dismiss': 'Ignorer',
    'match.matchComplete': 'Course terminee',
    'results.eyebrow': 'Resultats',
    'results.title': 'Le recapitulatif solo repose maintenant sur l’etat de course enregistre.',
    'results.description': 'Le temps ecoule, le nombre de sauts et la derniere page signalee sont conserves depuis la course active vers une surface de recap concise.',
    'results.multiplayerTitle': 'Les resultats multijoueurs reprennent maintenant le classement enregistre.',
    'results.multiplayerDescription': 'Classements, temps d’arrivee et abandons sont relus depuis l’etat de match en memoire pour terminer la partie sur un recap partage.',
    'results.elapsed': 'Temps',
    'results.hops': 'Sauts',
    'results.currentArticle': 'Page finale',
    'results.noRace': 'Aucune course solo terminee n’est disponible pour le moment.',
    'results.winner': 'Vainqueur',
    'results.playersFinished': 'Joueurs arrives',
    'results.playersAbandoned': 'Joueurs ayant abandonne',
    'results.route': 'Parcours',
    'results.state': 'Etat',
    'results.finalRanking': 'Classement final',
    'results.finishTime': 'Temps d’arrivee',
    'results.status': 'Statut',
    'results.stillRacing': 'Encore en course',
    'language.fr': 'Francais',
    'language.en': 'Anglais'
  }
};

@Injectable({ providedIn: 'root' })
export class LocalizationService {
  private readonly currentLanguage = signal<Locale>('fr');

  readonly language = computed(() => this.currentLanguage());

  setLanguage(language: string): void {
    this.currentLanguage.set(language === 'en' ? 'en' : 'fr');
  }

  t(key: TranslationKey): string {
    return translations[this.currentLanguage()][key];
  }
}

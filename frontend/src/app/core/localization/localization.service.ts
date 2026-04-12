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
  | 'match.joinInProgressTitle'
  | 'match.joinInProgressBody'
  | 'match.joinInProgressButton'
  | 'match.joinError'
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
    'app.strap': 'Race from any Wikipedia article to another — click links, follow your instincts, and reach the target before everyone else.',
    'home.eyebrow': 'Home',
    'home.title': 'How many clicks does it take to get there?',
    'home.description': 'Create a lobby, invite your friends, and race through Wikipedia by clicking links. The fastest path wins.',
    'home.create': 'Start a lobby',
    'home.create.button': 'Create Lobby',
    'home.create.creating': 'Creating...',
    'home.create.placeholder': 'Your temporary display name',
    'home.create.error': 'The lobby could not be created right now.',
    'lobby.eyebrow': 'Lobby',
    'lobby.title': 'Set up your race',
    'lobby.description': 'Share the invite link with your friends, pick your articles, and start when everyone is ready.',
    'lobby.loading': 'Loading lobby',
    'lobby.noSelection': 'No active lobby selected',
    'lobby.noSelection.body': 'Create a lobby from the home page to get a shareable invite link.',
    'lobby.back': 'Return home',
    'lobby.share': 'Invite Link',
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
    'lobby.hostLanguage': 'Wikipedia language',
    'lobby.hostLanguageHint': 'Pick the language your Wikipedia articles will be in.',
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
    'match.title': 'Navigate from article to article',
    'match.description': 'Click links inside Wikipedia articles to hop from your starting page to the target. Fewest hops wins!',
    'match.loading': 'Loading article view...',
    'match.error': 'The article view could not be loaded.',
    'match.openFromLobby': 'Go back to the lobby and pick a starting article to begin.',
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
    'match.multiplayerState': 'Multiplayer',
    'match.abandon': 'Abandon match',
    'match.viewer.empty': 'No source article is selected for this lobby yet.',
    'match.viewer.attribution': 'Content from Wikipedia',
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
    'match.joinInProgressTitle': 'Join this match',
    'match.joinInProgressBody': 'Enter a display name to join the race already in progress.',
    'match.joinInProgressButton': 'Join match',
    'match.joinError': 'The match could not be joined.',
    'results.eyebrow': 'Results',
    'results.title': 'Race summary',
    'results.description': 'Here\'s how your solo race went.',
    'results.multiplayerTitle': 'Final results',
    'results.multiplayerDescription': 'Here\'s how everyone finished.',
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
    'nav.results': 'Résultats',
    'app.kicker': 'WikiRacer',
    'app.strap': 'Parcourez Wikipédia de lien en lien — trouvez le chemin le plus court vers la page cible avant tout le monde !',
    'home.eyebrow': 'Accueil',
    'home.title': 'Combien de clics pour y arriver ?',
    'home.description': 'Créez un salon, invitez vos amis et partez à la course à travers Wikipédia. Le chemin le plus court gagne.',
    'home.create': 'Créer un salon',
    'home.create.button': 'Créer le salon',
    'home.create.creating': 'Création...',
    'home.create.placeholder': 'Votre nom temporaire',
    'home.create.error': 'Le salon ne peut pas être créé pour le moment.',
    'lobby.eyebrow': 'Salon',
    'lobby.title': 'Préparez votre course',
    'lobby.description': 'Partagez le lien d\u2019invitation avec vos amis, choisissez vos articles et lancez la partie quand tout le monde est prêt.',
    'lobby.loading': 'Chargement du salon',
    'lobby.noSelection': 'Aucun salon actif sélectionné',
    'lobby.noSelection.body': 'Créez un salon depuis la page d\u2019accueil pour obtenir un lien à partager.',
    'lobby.back': 'Retour à l\u2019accueil',
    'lobby.share': 'Lien d\u2019invitation',
    'lobby.copy': 'Copier le lien',
    'lobby.copied': 'Copié',
    'lobby.joined': 'Connecté en tant que',
    'lobby.joinForm': 'Rejoindre ce salon',
    'lobby.joinButton': 'Rejoindre',
    'lobby.joining': 'Connexion...',
    'lobby.members': 'Participants actuels',
    'lobby.joinState.open': 'Salon ouvert aux joueurs',
    'lobby.joinState.full': 'Salon complet',
    'lobby.language': 'Langue',
    'lobby.players': 'Joueurs',
    'lobby.code': 'Code du salon',
    'lobby.hostLanguage': 'Langue de Wikipédia',
    'lobby.hostLanguageHint': 'Choisissez la langue dans laquelle les articles Wikipédia seront affichés.',
    'lobby.changeLanguage': 'Appliquer la langue',
    'lobby.changeLanguagePending': 'Mise à jour...',
    'lobby.changeLanguageError': 'La langue du salon n\u2019a pas pu être mise à jour.',
    'lobby.searchStart': 'Rechercher la page source',
    'lobby.searchTarget': 'Rechercher la page cible',
    'lobby.searchPlaceholder': 'Rechercher des titres Wikipédia',
    'lobby.searching': 'Recherche...',
    'lobby.noSuggestions': 'Aucune suggestion de page jouable.',
    'lobby.pickSource': 'Utiliser comme source',
    'lobby.pickTarget': 'Utiliser comme cible',
    'lobby.randomizeSource': 'Tirer une source',
    'lobby.randomizeTarget': 'Tirer une cible',
    'lobby.randomizing': 'Tirage...',
    'lobby.selectionMode.manual': 'manuel',
    'lobby.selectionMode.random': 'aléatoire',
    'lobby.articleUpdateError': 'La sélection d\u2019article n\u2019a pas pu être enregistrée.',
    'lobby.selectedArticles': 'Articles sélectionnés',
    'lobby.source': 'Source',
    'lobby.target': 'Cible',
    'lobby.notSelected': 'Non sélectionné',
    'lobby.openViewer': 'Ouvrir le lecteur d\u2019articles',
    'lobby.startSolo': 'Démarrer une course solo',
    'lobby.ready': 'Prêt',
    'lobby.notReady': 'Pas prêt',
    'lobby.countdown': 'Compte à rebours',
    'lobby.countdownWaiting': 'En attente que tous les joueurs soient prêts.',
    'lobby.realtimeError': 'La synchronisation temps réel du salon est temporairement indisponible.',
    'lobby.startMultiplayer': 'Démarrer la partie multijoueur',
    'lobby.openMultiplayer': 'Ouvrir la partie multijoueur',
    'match.eyebrow': 'Course',
    'match.title': 'Naviguez d\u2019article en article',
    'match.description': 'Cliquez sur les liens dans les articles Wikipédia pour aller de la page de départ à la cible. Moins de sauts, c\u2019est mieux !',
    'match.loading': 'Chargement de l\u2019article...',
    'match.error': 'Le lecteur d\u2019article n\u2019a pas pu être chargé.',
    'match.openFromLobby': 'Retournez dans le salon et choisissez un article de départ pour commencer.',
    'match.source': 'Article source',
    'match.target': 'Article cible',
    'match.sourceLink': 'Page Wikipédia originale',
    'match.startRace': 'Démarrer la course solo',
    'match.restartRace': 'Relancer la course solo',
    'match.elapsed': 'Temps',
    'match.hops': 'Sauts',
    'match.currentArticle': 'Page courante',
    'match.status.active': 'Active',
    'match.status.finished': 'Terminée',
    'match.finishBanner': 'Destination atteinte. La course solo est terminée.',
    'match.viewResults': 'Voir les résultats',
    'match.players': 'Joueurs',
    'match.multiplayerStartError': 'La partie multijoueur n\u2019a pas pu être démarrée.',
    'match.multiplayerState': 'Multijoueur',
    'match.abandon': 'Abandonner la partie',
    'match.viewer.empty': 'Aucun article source n\u2019est encore sélectionné pour ce salon.',
    'match.viewer.attribution': 'Contenu provenant de Wikipédia',
    'match.viewer.externalNotice': 'Les liens externes s\u2019ouvrent dans un nouvel onglet.',
    'match.toc': 'Sommaire',
    'match.raceStatus': 'État de la course',
    'match.player.you': 'vous',
    'match.status.abandoned': 'Abandonné',
    'match.status.disconnected': 'Déconnecté',
    'match.reachedTarget': 'a atteint la cible',
    'match.leftTheRace': 'a abandonné la course',
    'match.youFinished': 'Vous avez atteint la cible !',
    'match.place': 'Rang',
    'match.dismiss': 'Ignorer',
    'match.matchComplete': 'Course terminée',
    'match.joinInProgressTitle': 'Rejoindre cette partie',
    'match.joinInProgressBody': 'Entrez un nom pour rejoindre la course déjà en cours.',
    'match.joinInProgressButton': 'Rejoindre la partie',
    'match.joinError': 'La partie n\u2019a pas pu être rejointe.',
    'results.eyebrow': 'Résultats',
    'results.title': 'Récapitulatif de la course',
    'results.description': 'Voilà comment s\u2019est passée votre course solo.',
    'results.multiplayerTitle': 'Résultats finaux',
    'results.multiplayerDescription': 'Voilà comment tout le monde a terminé.',
    'results.elapsed': 'Temps',
    'results.hops': 'Sauts',
    'results.currentArticle': 'Page finale',
    'results.noRace': 'Aucune course solo terminée n\u2019est disponible pour le moment.',
    'results.winner': 'Vainqueur',
    'results.playersFinished': 'Joueurs arrivés',
    'results.playersAbandoned': 'Joueurs ayant abandonné',
    'results.route': 'Parcours',
    'results.state': 'État',
    'results.finalRanking': 'Classement final',
    'results.finishTime': 'Temps d\u2019arrivée',
    'results.status': 'Statut',
    'results.stillRacing': 'Encore en course',
    'language.fr': 'Français',
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

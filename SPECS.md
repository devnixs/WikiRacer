# Wikiracer Clone Specifications

## 1. Purpose

Build a production-grade clone of the Wikiracer game where players race from a starting Wikipedia article to a target article by navigating only through valid internal Wikipedia links.

This document defines the functional, technical, architectural, and non-functional specifications for a long-term scalable implementation.

## 2. Goals

- Deliver a real-time multiplayer Wikiracer experience.
- Use a C# backend on .NET 10.
- Use WebSockets for real-time game communication.
- Keep all runtime state in memory.
- Build the frontend in Angular.
- Design the system so the codebase remains maintainable and scalable as the project grows.
- Avoid short-term optimizations that create architectural debt.

## 3. Non-Goals

- No relational or NoSQL database in the initial version.
- No persistence of matches, users, or analytics across restarts.
- No native mobile application in the initial version.
- No CMS or admin back office in the initial version.
- No direct editing of Wikipedia content.

## 4. Product Overview

The platform allows users to:

- Create and join game lobbies.
- Start solo or multiplayer races.
- Receive a start page and a target page.
- Navigate article-to-article through Wikipedia pages.
- See live race progress over WebSockets.
- Finish when they reach the target page according to server-side match tracking.

The system must support multiple concurrent games and must be structured to evolve toward larger scale without a rewrite.

## 5. Core Gameplay Rules

### 5.1 Match Definition

A match consists of:

- A unique match identifier.
- A game mode.
- A chosen Wikipedia language edition, with `fr` as the initial supported edition.
- A chosen UI language for the client experience, initially coupled to the Wikipedia language edition unless later decoupled by product requirements.
- A start article.
- A target article.
- A set of players.
- A match state machine.
- A server-side event timeline kept in memory.

### 5.2 Player Objective

The player must reach the target article from the start article by navigating through Wikipedia pages in the selected language edition.

### 5.3 Link Validity Rules

Version 1 does not require server-side validation of each clicked link or navigation step.

Gameplay assumptions for version 1:

- The client reports the player’s current Wikipedia page as they browse.
- The server records reported page transitions for match progression and multiplayer visibility.
- The server may normalize reported article titles and redirects for consistency, but it does not need to verify that the player arrived there through a valid clicked link.

Version 1 simplifications:

- The game does not attempt to detect manual URL editing.
- The game does not attempt to reject browser history navigation.
- The game does not attempt to prove that the reported page was reached from the previous page through a valid internal link.

The backend should still normalize titles and resolve redirects where useful for consistent display and finish detection.

### 5.4 Win Condition

A player wins when the server confirms that their current reported article matches the normalized target article.

When a player reaches the destination:

- That player must be marked as finished in the match state.
- The finish must be announced in the UI to all match participants.
- Other players must be allowed to continue playing if they want to keep racing for placement.
- Other players must also be allowed to abandon the race voluntarily.

### 5.5 Game Modes

Initial required modes:

- Solo practice.
- Private multiplayer lobby.
- Timed multiplayer race.

Future-ready modes to support in the design:

- Ranked matchmaking.
- Team races.
- Daily challenge.
- Tournament brackets.

## 6. User Roles

### 6.1 Anonymous Player

Can:

- Open the application.
- Create or join a temporary lobby.
- Play without persistent account storage.

### 6.2 Future Authenticated Player

Not required in version 1, but the architecture must leave room for:

- Identity integration.
- Profiles.
- Ratings.
- Match history.

## 7. Functional Requirements

### 7.1 Lobby Management

The system must allow:

- Creating a lobby.
- Generating a shareable lobby URL.
- Joining a lobby directly from a shareable URL.
- Optionally exposing a short lobby code as a fallback join mechanism, but not as the primary sharing flow.
- Leaving a lobby.
- Assigning a host.
- Reassigning host if the host disconnects.
- Configuring match options before start.

Lobby settings should include:

- Language edition.
- UI language.
- Time limit.
- Player cap.
- Source page.
- Destination page.
- Source page selection mode: manual search or curated random.
- Destination page selection mode: manual search or curated random.

### 7.1.3 Lobby Sharing and Joining

Sharing a lobby with friends must be as simple as sharing a URL.

Version 1 primary behavior:

- Each lobby must have a canonical shareable URL.
- Opening the URL in a browser must take the player directly into the lobby join flow for that specific room.
- If the player is already identified in the current runtime session, opening the URL should take them directly into the lobby without unnecessary intermediate steps.
- If the player is not yet identified in the current runtime session, the app may ask for the minimal temporary player information required to join, then continue into the lobby automatically.

Required product behaviors:

- The host must have an obvious copyable share link in the lobby UI.
- The share link must remain stable for the lifetime of the in-memory lobby.
- The backend must validate that the referenced lobby still exists and is joinable.
- If the lobby is expired, full, or already started beyond the allowed join state, the user must receive a clear error state from the URL entry path.
- The URL entry flow must preserve the target lobby context through app bootstrap and connection establishment.

Routing and identifier requirements:

- The frontend must support route-based lobby entry such as `/lobby/{publicLobbyId}` or an equivalent stable URL scheme.
- The URL must use a public-safe lobby identifier and must not expose internal implementation details.
- Public lobby identifiers must be hard to guess and suitable for direct sharing.
- The internal domain model may still keep a lobby code or separate identifier object, but the shared URL must be treated as the canonical friend-invite mechanism.

### 7.1.0 Language Selection

The lobby must allow the host to choose the language in which the game is played.

This language selection must affect both:

- The language of the Wikipedia edition used for source page, destination page, search suggestions, normalization, redirects, and gameplay browsing context.
- The game UI language shown to players in the Angular frontend.

Version 1 rule:

- The Wikipedia language edition and the UI language must be selected together as a single lobby setting.

Future-ready rule:

- The architecture must allow decoupling content language and UI language later without redesigning the core match domain.

Required behaviors:

- The host can choose the game language before the match starts.
- The selected language is broadcast in real time to all lobby participants.
- Changing the language invalidates previously selected source and destination articles unless they are explicitly resolved again for the new Wikipedia edition.
- Search suggestions and randomization must always operate in the currently selected language edition.
- Match state, contracts, and logs must retain the selected language as part of match context.

### 7.1.1 Source and Destination Selection

The lobby host must be able to configure both the source page and the destination page before the match starts.

Required behaviors:

- The host can set the source page manually.
- The host can set the destination page manually.
- The host can use a searchable dropdown for source page selection.
- The host can use a searchable dropdown for destination page selection.
- The host can click a `Randomize` action independently for source and destination.
- The selected articles must be broadcast in real time to all lobby participants.
- Only the host may modify these values once the lobby is created, unless future permission rules are added.

Manual search requirements:

- Search must provide article suggestions as the host types.
- Suggestions must be backed by server-side Wikipedia article lookup.
- Suggestions must exclude invalid namespaces and non-playable pages.
- Suggestions must be scoped to the currently selected Wikipedia language edition.
- The final selected article must be normalized and resolved by the backend before being accepted into lobby state.

Curated random requirements:

- `Randomize` must not return arbitrary obscure pages.
- `Randomize` must select from a curated eligible pool of well-known articles.
- Source and destination randomization must use the same eligibility framework, with optional future support for different difficulty tiers.
- The backend must reject a random pair that is identical after normalization.
- The backend should reject or reroll pairs that are obviously poor gameplay candidates, such as extremely trivial pairs or unreachable pairs if such validation becomes available.

### 7.1.2 Well-Known Random Article Eligibility

The project must define and enforce a server-side heuristic that estimates whether a Wikipedia page is broadly recognizable enough to be eligible for `Randomize`.

The heuristic should be based on multiple signals rather than a single metric.

Candidate signals include:

- Number of language editions in which the article exists.
- Number of backlinks from other Wikipedia articles.
- Number of outgoing links.
- Pageview popularity over a defined time window.
- Article quality or completeness proxies such as page length, category density, or metadata richness.
- Exclusion of disambiguation pages, lists, maintenance pages, date pages, and overly niche namespaces.

Recommended version 1 strategy:

- Build an `IPlayableArticleSelector` abstraction in the backend.
- Compute a weighted eligibility score from available Wikipedia metadata.
- Define a minimum eligibility threshold for inclusion in the random pool.
- Prefer articles above a stronger familiarity threshold rather than barely passing the minimum.
- Maintain a denylist of categories or patterns known to produce poor game pages.
- Optionally maintain a curated allowlist overlay for especially recognizable topics.

Recommended scoring direction:

- Use language-count as a strong proxy for global recognizability.
- Use backlink count as a strong proxy for article centrality in the graph.
- Use recent pageviews as a strong proxy for real-world familiarity.
- Treat a page as random-eligible only when it satisfies at least two strong signals or one exceptional signal plus structural validity checks.

When gameplay is configured for a specific language edition, the selector should still prefer articles that are recognizable within that language context while also giving weight to cross-language availability as a signal of general familiarity.

The heuristic must be replaceable without changing match-domain logic.

### 7.2 Match Lifecycle

The backend must model the match lifecycle explicitly:

- `Created`
- `WaitingForPlayers`
- `Ready`
- `StartingCountdown`
- `InProgress`
- `Finished`
- `Cancelled`
- `Expired`

State transitions must be validated and centralized.

The lifecycle design must distinguish between:

- Match-level state.
- Per-player race status.

Minimum per-player statuses:

- `Active`
- `Finished`
- `Abandoned`
- `Disconnected`

Version 1 match-completion behavior:

- A multiplayer match does not end immediately when the first player finishes.
- The match remains in progress while other players continue, unless an explicit completion rule is met.
- The match may transition to `Finished` when all players are finished, abandoned, or otherwise no longer actively racing, or when a configured time limit expires.

### 7.3 Real-Time Communication

The frontend and backend must communicate primarily through WebSockets for:

- Lobby membership events.
- Match countdowns.
- Match start.
- Player progress updates.
- Player finished events.
- Player abandoned events.
- Presence and connection status.
- Match finish events.
- Server notifications.

HTTP endpoints may be used for:

- Initial app bootstrap.
- Health checks.
- Static configuration.
- Optional fallback reads that are not latency sensitive.

### 7.4 Article Navigation and Progress Tracking

The backend must:

- Track each player’s current reported article.
- Maintain the path taken by each player.
- Prevent duplicate event application from retried client messages.
- Be resilient to out-of-order messages.

Version 1 does not require the backend to:

- Validate that a reported move came from a legal clickable link on the previous page.
- Reject reported jumps between unrelated articles.
- Enforce link-by-link navigation integrity.

### 7.4.1 Multiplayer Browsing Visibility

During an active multiplayer match, players must be able to see what pages other players are currently browsing.

Version 1 required behavior:

- The server must track the current reported article for every active player.
- The server must broadcast player article changes in real time to other players in the same match.
- The frontend must display each player’s current reported page during the race.
- The displayed page for another player must reflect the latest server-recorded state for that player.

Recommended version 1 presentation:

- Show each player with their current article title.
- Show each player’s hop count alongside the current article.
- Update the display incrementally as navigation events are reported.

Data visibility rules:

- Players should be able to see the current reported article of all other active players in the same match.
- The server may also expose the full reported path for each player to the match participants, but this is optional in version 1.
- Presence and disconnection state must remain visible alongside the browsing state.

### 7.5 Spectator Support

Version 1 should NOT include architecture support for spectators.

### 7.6 Reconnection

The system must support reconnecting a disconnected player to an ongoing in-memory match as long as the match still exists in memory.

Required behaviors:

- Resume by reconnect token or equivalent transient identity.
- Rebind WebSocket session to existing player state.
- Replay the minimum required snapshot and recent events.

### 7.7 Match Results

Since no database is allowed, results exist only in memory during runtime.

The system must still produce a clear result structure:

- Winner.
- Final ranking.
- Finish times.
- Paths taken for each players.
- Abandon, disconnection, or forfeit status.

### 7.7.1 Finish Announcement and Continue-or-Abandon Flow

When a player reaches the destination page, the game must visibly communicate that event to all players without forcing the others out of the race.

Required behavior:

- The UI must show that a player has reached the destination.
- The finishing player must be visually marked as finished in the player list or race-status area.
- Other active players must remain able to continue browsing and improve their final placement.
- Other active players must have an explicit option to abandon the race.
- A player who abandons must be marked accordingly in the UI for all participants.

Recommended version 1 UI behavior:

- Show a clear finish notification when a player reaches the destination.
- Keep the race-status area updated with each player’s status, current page, hop count, and finish or abandon state.
- Do not block article reading and navigation for players who choose to continue.

## 8. External Dependency Requirements

### 8.1 Wikipedia Data Access

The backend must integrate with Wikipedia or a compatible source to:

- Resolve article metadata.
- Normalize titles.
- Resolve redirects.
- Search for candidate articles by title prefix or query.
- Retrieve metadata required for random article eligibility scoring.
- Resolve language-specific article data for the selected Wikipedia edition.
- Retrieve article HTML or equivalent renderable content suitable for in-app display.

### 8.1.1 Article Search and Random Selection Data

The Wikipedia integration boundary must support two separate concerns:

- Manual article search for lobby configuration.
- Curated random article selection for recognizable start and destination pages.

For manual article search, the provider should expose:

- Search suggestions by query text.
- Canonical article title resolution.
- Namespace and article-type filtering.
- Explicit language-scoped querying.

For curated random selection, the provider should expose or derive:

- Language-count information when available.
- Backlink or incoming-link estimates when available.
- Pageview metrics when available.
- Structural page metadata needed to exclude poor candidates.
- Eligibility filtered per selected gameplay language edition.

If live Wikipedia APIs do not expose every desired signal efficiently, the architecture must allow a hybrid provider strategy such as:

- Live API for title search and normalization.
- In-memory cached scoring snapshots for random-eligible article pools.
- Offline preprocessing pipeline introduced later without breaking application contracts.

### 8.1.2 Article Rendering Strategy

The game UI must display Wikipedia article content inside the application in a way that supports long-term product control over navigation, layout, and instrumentation.

Recommended version 1 strategy:

- Do not use a raw cross-origin `iframe` of the standard Wikipedia article page as the primary gameplay rendering approach.
- Fetch article HTML through Wikimedia or MediaWiki APIs.
- Render the article inside the Angular application using a controlled article viewer.
- Rewrite internal Wikipedia links into application-controlled navigation actions.

Reasoning:

- A raw `iframe` provides poor control over navigation interception, overlays, multiplayer UI, and future custom presentation.
- A raw `iframe` also leaves the product dependent on Wikipedia’s live page chrome and layout decisions.
- API-driven rendering allows the application to keep a stable game UI while still showing real article content.
- API-driven rendering makes it possible to integrate current-player state, other-player visibility, language switching, and analytics without fighting browser cross-origin boundaries.

Implementation requirements:

- The backend Wikipedia integration must expose an article-rendering payload or normalized HTML payload for the frontend.
- The rendering pipeline must preserve article readability while removing or adapting elements that are not useful in gameplay chrome.
- Internal article links must be rewritten so that clicking them stays inside the application and updates match progress tracking.
- External links should either open outside the game shell or be visibly marked as out-of-game navigation.
- The rendering pipeline must support language-specific article retrieval.

Security and robustness requirements:

- Retrieved HTML must be sanitized before rendering in the Angular application.
- The rendering strategy must not rely on trusting arbitrary third-party scripts from Wikipedia pages.
- The application must avoid executing remote page scripts inside the gameplay surface.
- The renderer must tolerate article shape differences across languages and templates.

Legal and attribution requirements:

- The in-app article viewer must preserve required attribution and license information for reused Wikipedia content.
- The application must provide a clear link back to the original Wikipedia article.
- The application must retain enough source metadata to satisfy attribution obligations for reused content.

The integration layer must be abstracted behind interfaces so it can later be swapped for:

- Wikipedia API access.
- Preprocessed dumps.
- Cached graph services.

The core domain must not depend directly on a specific Wikipedia transport format.

### 8.2 Rate Limiting and Dependency Protection

Because state is in memory and the application may depend on external Wikipedia data, the system must include:

- Request throttling.
- Concurrency controls.
- Bounded retries.
- Timeouts.
- Circuit breaker behavior at the infrastructure boundary.

## 9. Non-Functional Requirements

### 9.1 Scalability

The project must be designed for growth in both codebase complexity and runtime load.

Required design principles:

- Strict separation between domain, application, infrastructure, and transport layers.
- Clear bounded contexts even if initially hosted in one deployable.
- Message contracts versioned from the start.
- In-memory repositories hidden behind interfaces.
- Stateless connection handlers except for session binding concerns.
- Match logic isolated from transport and external APIs.

### 9.2 Maintainability

The codebase must:

- Use consistent architectural patterns.
- Enforce clear folder and project boundaries.
- Prefer explicit domain models over weakly typed structures.
- Use unit and integration tests as first-class project assets.
- Document public contracts and important invariants.

### 9.3 Reliability

The system must:

- Handle malformed client messages safely.
- Reject unauthorized or invalid transitions.
- Avoid process-wide failures from single-match faults.
- Apply cancellation tokens consistently.
- Protect shared in-memory structures from race conditions.

### 9.4 Performance

Target characteristics:

- Low-latency propagation of live match events.
- Efficient in-memory tracking for many concurrent matches.
- Minimal lock contention in hot paths.
- Controlled memory growth.

### 9.5 Observability

The application must include:

- Structured logs.
- Metrics.
- Distributed tracing readiness.
- Correlation identifiers per connection and match.
- Diagnostic events around websocket traffic and external dependency calls.

### 9.6 Security

Even without persistent accounts, the system must include:

- Input validation on all HTTP and WebSocket payloads.
- Origin and CORS restrictions.
- Connection-level abuse protection.
- Lobby join code entropy strong enough to prevent trivial guessing.
- Server-side controls for match state, connection integrity, and abuse prevention.

## 10. Proposed High-Level Architecture

### 10.1 Solution Shape

Recommended monorepo structure:

- `backend/`
- `frontend/`
- `docs/`
- `ops/`

Recommended backend solution layout:

- `WikiRacer.Api` for HTTP and WebSocket transport.
- `WikiRacer.Application` for use cases and orchestration.
- `WikiRacer.Domain` for aggregates, value objects, domain services, and rules.
- `WikiRacer.Infrastructure` for Wikipedia integration, in-memory repositories, logging, metrics, and runtime adapters.
- `WikiRacer.Contracts` for DTOs and message contracts shared across transport boundaries.
- `WikiRacer.Tests.Unit`
- `WikiRacer.Tests.Integration`
- `WikiRacer.Tests.Architecture`

Recommended frontend layout:

- Angular app with feature-based modules or standalone feature areas.
- Dedicated layers for core services, websocket client, state management, shared UI, and feature domains.

### 10.2 Architectural Style

Recommended style:

- Modular monolith initially.
- Internal boundaries designed so extraction into services remains feasible later.

This is the correct tradeoff because:

- There is no database in v1, reducing immediate benefit from microservices.
- Gameplay requires tight consistency within a running process.
- Scalability needs can be addressed first through clean boundaries, horizontal app instances, and partitioning strategies before service decomposition.

### 10.3 Backend Layer Responsibilities

#### Domain Layer

Owns:

- Match aggregate.
- Lobby aggregate or equivalent root.
- Player session concepts.
- Navigation attempt rules.
- Match state transitions.
- Win and ranking rules.

Must not depend on:

- ASP.NET Core.
- WebSocket frameworks.
- Wikipedia SDK specifics.
- Logging implementations.

#### Application Layer

Owns:

- Commands and queries.
- Use-case orchestration.
- Idempotency policies.
- Transaction-like coordination over in-memory stores.
- Publication of domain events to transport adapters.

#### Infrastructure Layer

Owns:

- In-memory repository implementations.
- Wikipedia API client or adapter.
- Background cleanup jobs.
- Metrics exporters.
- Logging setup.
- Resilience policies.

#### API Layer

Owns:

- HTTP endpoints.
- WebSocket endpoint lifecycle.
- Authentication placeholder hooks.
- Connection/session mapping.
- Message serialization and version routing.

## 11. In-Memory Data Strategy

### 11.1 General Rules

All runtime state must remain in memory.

This includes:

- Lobbies.
- Matches.
- Active players.
- WebSocket connection mappings.
- Event buffers needed for reconnection.
- Article metadata caches.

### 11.2 Repository Abstractions

In-memory implementations must still be hidden behind interfaces so later replacements remain possible without rewriting business logic.

Examples:

- `IMatchRepository`
- `ILobbyRepository`
- `IPlayerSessionRepository`
- `IArticleGraphProvider`
- `IEventBuffer`

### 11.3 Concurrency Requirements

Because the application is multi-user and real-time, the in-memory model must be concurrency-safe.

Requirements:

- Avoid ad hoc shared mutable state scattered across handlers.
- Use aggregate-level synchronization strategy deliberately.
- Define whether locking is per match, per lobby, or actor-style mailbox based.
- Ensure consistent ordering of player actions within a match.

Recommended initial strategy:

- Single-writer coordination per match instance.
- Concurrent access for reads through immutable snapshots or controlled copies.

### 11.4 Eviction and Cleanup

Since memory is finite, the system must implement expiration policies for:

- Empty lobbies.
- Finished matches.
- Disconnected sessions.
- Temporary reconnection tokens.
- Article cache entries.

Cleanup must be:

- Time-based.
- Observable via metrics.
- Safe against removal of active sessions.

## 12. WebSocket Protocol Requirements

### 12.1 Protocol Principles

The protocol must be:

- Explicitly versioned.
- Contract-driven.
- Backward-aware where practical.
- Independent from Angular-specific concerns.

### 12.2 Message Shape

Every message should contain:

- Message type.
- Protocol version.
- Correlation identifier.
- Match or lobby identifier where applicable.
- Payload.
- Server timestamp where applicable.

### 12.3 Client-to-Server Message Types

Minimum required categories:

- Connect and resume session.
- Create lobby.
- Join lobby.
- Leave lobby.
- Update lobby settings.
- Start match.
- Submit navigation attempt.
- Heartbeat or ping.

### 12.4 Server-to-Client Message Types

Minimum required categories:

- Connection accepted.
- Error and validation failure.
- Lobby snapshot.
- Lobby updated.
- Match countdown started.
- Match started.
- Player progress updated.
- Player current article updated.
- Player finished.
- Player abandoned.
- Match finished.
- Presence changed.
- Server notification.

### 12.5 Delivery Semantics

The protocol should support:

- Client message identifiers for deduplication.
- Snapshot plus incremental update patterns.
- Clear reconnect and resync flow.

No assumption should be made that the browser sends each action exactly once.

## 13. Backend Technical Specifications

### 13.1 Platform

- C#
- .NET 10
- ASP.NET Core
- Native WebSocket support or an abstraction built on top of it, without compromising control over contracts and performance

### 13.2 Coding Standards

Required standards:

- Nullable reference types enabled.
- Treat warnings as errors in CI where feasible.
- Strong analyzers enabled.
- Explicit async and cancellation handling.
- No business logic in controllers or WebSocket endpoint classes.
- No direct infrastructure calls from UI transport handlers.

### 13.3 Domain Modeling

The backend must favor:

- Value objects for article titles, lobby codes, match identifiers, player identifiers, and navigation steps.
- Rich domain entities and aggregates.
- Explicit invariants enforced close to the model.

Avoid:

- Primitive obsession.
- Anemic domain models for gameplay rules.
- Generic dictionary-based state blobs.

### 13.4 Background Processing

The backend will need hosted services for:

- Cleanup of stale in-memory state.
- External article cache refresh or eviction.
- Timeout checks for matches and lobbies.

These background workers must remain infrastructure concerns and not own core game rules.

## 14. Frontend Technical Specifications

### 14.1 Platform

- Angular, current stable release at implementation time
- TypeScript with strict mode enabled
- RxJS for reactive stream handling

### 14.2 Frontend Capabilities

The frontend must provide:

- Landing flow.
- Lobby creation and join flow.
- URL-based lobby entry flow.
- Copyable lobby share link in the lobby room.
- Game language selection in the lobby.
- Source and destination article pickers in the lobby.
- Searchable dropdown article selection for the host.
- Independent `Randomize` actions for source and destination.
- Lobby room UI.
- Match play UI.
- In-app Wikipedia article viewer for gameplay.
- Real-time player progress UI.
- Real-time display of other players’ current pages during a match.
- Match timer showing elapsed time.
- Hop count display for each player.
- Player finish state display.
- Player abandon action and abandon state display.
- Table of contents navigation for the current article.
- End-of-match summary UI.
- Reconnection-aware client behavior.

### 14.2.1 Match Screen Layout

The in-game UI must have a clearly defined gameplay layout rather than a generic article page with overlays.

Required information visible during gameplay:

- The current article content.
- The current elapsed time for the match.
- The other players’ current pages.
- The hop count for each player.
- The finish or abandon status of players who are no longer actively racing.
- The current article table of contents.

Desktop layout requirements:

- The main content area must show the current Wikipedia page content.
- A left-side panel must display the table of contents for the current article.
- A race-status area must show the other players, their current pages, their hop counts, and their finish or abandon state.
- The elapsed timer must remain clearly visible without requiring scrolling through the article.

Mobile layout requirements:

- The main content area must still prioritize article readability.
- The table of contents must be collapsible.
- The race-status area must remain accessible without overwhelming the article content.
- The elapsed timer and at least a compact summary of player progress must remain visible in a mobile-appropriate way.

Interaction requirements:

- Selecting an item in the table of contents must scroll or jump to the relevant section in the rendered article.
- The match screen must handle long article pages without losing access to key race information.
- The display of other players’ current pages and hop counts must update in real time.
- When a player finishes, the UI must announce it without forcing other players out of the gameplay screen.
- Players who have not finished must be able to explicitly abandon from the match screen.

### 14.2.2 Visual Design Direction

The UI should have a clean, refined, visually striking presentation.

Design intent:

- The overall design should feel `epure`, deliberate, and high-end rather than cluttered.
- The interface should use vivid colors with confidence, while keeping readability and hierarchy strong.
- The result should feel memorable and polished rather than generic dashboard styling.

Visual requirements:

- Use a restrained layout with clear spacing and strong alignment.
- Use a vivid color system to create energy and identity.
- Keep typography crisp and intentional.
- Avoid muddy palettes, low-contrast washed-out surfaces, or overly utilitarian enterprise styling.
- Avoid making the experience look like a plain embedded encyclopedia page with game widgets around it.

Recommended styling direction:

- Pair clean structural surfaces with bold accent colors.
- Make the race-status area and major interactive elements visually distinctive.
- Use color intentionally to separate article reading, race information, and key actions.
- Preserve calm reading conditions for the article body even while the surrounding shell is vibrant.

Quality bar:

- The interface should look polished enough to feel like a premium product.
- Visual design decisions should be cohesive across lobby, match, and results screens.
- The UI should aim to look genuinely impressive, not merely functional.

### 14.3 Frontend Architecture

The Angular application should be organized by features, not by file type only.

Recommended areas:

- `core/` for bootstrap, configuration, interceptors, websocket gateway, and app-wide services
- `shared/` for reusable UI and utilities
- `features/lobby/`
- `features/match/`
- `features/home/`
- `features/results/`

State management requirements:

- Use a predictable reactive state model.
- Keep websocket event handling isolated from presentation components.
- Separate server DTOs from view models.
- Keep selected game language in shared reactive state so search, randomization, and UI localization stay synchronized.
- Keep article navigation state isolated from raw rendering concerns so the viewer can evolve without affecting match orchestration.

The exact state library may be chosen later, but the architecture must support:

- Local feature state.
- Derived selectors.
- Replay and resync after reconnect.

### 14.4 UX Expectations

The UX must prioritize:

- Clear indication of the currently selected game language.
- Frictionless joining of a shared lobby from a URL.
- Fast comprehension of start and target pages.
- Clear feedback for invalid actions.
- Real-time visibility into other players’ progress.
- Clear visibility into the current page each other player is on.
- Continuous visibility of the elapsed timer.
- Continuous visibility of player hop counts.
- Clear visibility when a player has finished or abandoned.
- Robust reconnect messaging.
- Accessibility and keyboard navigation.

Visual UX expectations:

- The product should feel elegant and visually disciplined.
- Vivid color should enhance excitement and clarity, not create noise.
- Important race information should stand out immediately through layout, color, and hierarchy.
- The gameplay shell should feel custom-designed rather than template-driven.

Article viewer expectations:

- The article content must feel native to the game UI rather than an unrelated embedded website.
- Internal article navigation must be fast and visually consistent.
- The application should keep important game chrome visible while the article is being read.
- The viewer should support future enhancements such as highlighting the current title, breadcrumbs, path history, and optional game-specific annotations.
- On desktop, the table of contents should remain persistently available in the left panel.
- On mobile, the table of contents should be collapsible and easy to reopen.

Localization requirements:

- The Angular frontend must support runtime localization for supported game languages.
- All player-visible system text used during the match flow must be translatable.
- Language changes made in the lobby before match start must update the UI consistently without stale strings or stale article suggestions.

## 15. API and Contract Design

### 15.1 Versioning

Message and HTTP contracts must be versioned from the start.

Recommended pattern:

- URL or namespace versioning for HTTP.
- Explicit protocol version field for WebSocket messages.

### 15.2 Validation

All external DTOs must be validated before entering application use cases.

Validation rules must cover:

- Required fields.
- Length and format constraints.
- Enum validity.
- Replay or duplicate identifiers.
- Match state preconditions.

### 15.3 Error Model

The system must expose machine-readable errors with:

- Stable error codes.
- Human-readable message.
- Correlation identifier where useful.

## 16. Testing Strategy

Testing is mandatory and part of the specification.

### 16.1 Backend Tests

Required layers:

- Unit tests for domain rules.
- Application tests for use cases.
- Integration tests for WebSocket flows.
- Contract tests for serialized messages.
- Architecture tests to enforce project boundaries.
- Load or soak tests for real-time concurrency scenarios.

High-priority backend test scenarios:

- Join lobby from canonical share URL identifier.
- Language change updates the effective Wikipedia edition for search, randomization, and reported gameplay context.
- Redirect normalization.
- Reported page progression is recorded in order for a single player session.
- Broadcast of current reported article changes to other match participants.
- First player finish does not immediately terminate the match for others.
- Player abandon state is recorded and broadcast correctly.
- Duplicate client message handling.
- Reconnection during active match.
- Host disconnect and reassignment.
- Match finish race conditions.
- Cleanup of expired state.
- Manual source and destination article selection validation.
- Language change invalidates or re-resolves existing source and destination selections correctly.
- Randomized article eligibility scoring and filtering.
- Rejection of disambiguation or otherwise non-playable random pages.
- Reroll when source and destination normalize to the same article.
- Article HTML retrieval, sanitization, and link rewriting.

### 16.2 Frontend Tests

Required layers:

- Unit tests for state and services.
- Component tests for critical flows.
- End-to-end tests for core journey coverage.

High-priority frontend test scenarios:

- Create and join lobby.
- Open a shared lobby URL and join the correct room.
- Copy and reuse the canonical lobby share link.
- Change the lobby language and verify UI localization updates.
- Search and select source and destination pages from the lobby UI.
- Verify article search results are scoped to the selected language.
- Randomize source and destination pages from the lobby UI.
- Start match.
- Receive and render live progress.
- Receive and render other players’ current reported pages during the race.
- Render elapsed time and per-player hop counts during gameplay.
- Show when a player finishes and allow remaining players to continue.
- Allow a player to abandon and reflect that state in the shared UI.
- Render the table of contents in a left panel on desktop and as a collapsible panel on mobile.
- Render article content in the in-app viewer and navigate through rewritten internal links.
- Recover from websocket disconnect.
- Display match completion state correctly.

## 17. Deployment and Runtime Considerations

### 17.1 Initial Deployment Model

Initial deployment can be a single backend instance plus the Angular frontend, but the architecture must document the limitation clearly:

- In-memory state means horizontal scaling is constrained unless session and match ownership are partitioned.

### 17.2 Future Scaling Path

The design must preserve a path toward:

- Sticky routing to the owning backend instance.
- Match partitioning or sharding.
- Distributed cache or durable event store if persistence is added later.
- Dedicated graph or article metadata service.
- Dedicated matchmaking service.

### 17.3 Configuration

Configuration must be externalized for:

- Allowed origins.
- WebSocket settings.
- Wikipedia integration settings.
- Cache and cleanup TTLs.
- Lobby and match limits.
- Rate limiting thresholds.

## 18. Observability Requirements

The implementation must expose:

- Health endpoints.
- Readiness and liveness signals.
- Metrics for active connections, active lobbies, active matches, reported page transitions, external dependency latency, cache hit rate, and reconnect success rate.
- Structured logs enriched with match ID, lobby ID, player ID, and connection ID where relevant.

## 19. Security and Abuse Prevention

The initial version must protect against:

- Message flooding.
- Oversized payloads.
- Invalid lobby code brute force.
- Client-side cheating through forged progress messages.
- Replay of client actions.

Recommended protections:

- Per-connection rate limiting.
- Payload size limits.
- Join-code entropy and expiration.
- Server-side sequence or deduplication tracking.

## 20. Delivery Milestones

### Milestone 1: Foundation

- Create repository structure.
- Set up .NET 10 solution with layered projects.
- Set up Angular workspace.
- Define contracts and architecture tests.
- Establish CI baseline and coding standards.

### Milestone 2: Core Single-Player Flow

- Wikipedia article integration.
- Language selection wired through backend contracts and Angular localization.
- Start and target article selection.
- Host-controlled source and destination selection with search and curated randomization.
- In-app article viewer fed by API-driven Wikipedia content retrieval.
- Reported page tracking.
- Solo game loop.

### Milestone 3: Real-Time Lobby and Multiplayer

- Lobby lifecycle.
- WebSocket protocol.
- Match countdown.
- Live multiplayer progress.
- First-finisher announcement plus continue-or-abandon flow for remaining players.
- Finish detection and ranking.

### Milestone 4: Hardening

- Reconnection flow.
- Cleanup jobs.
- Metrics and structured logging.
- Load testing and profiling.
- Security and abuse protections.

### Milestone 5: Scalability Readiness

- Partitioning strategy documented.
- Internal extension points reviewed.
- Contract versioning validated.
- Performance hotspots addressed.

## 21. Acceptance Criteria

The project will be considered compliant with this specification when:

- A .NET 10 backend using WebSockets powers real-time gameplay.
- All application state is kept in memory only.
- An Angular frontend supports the main lobby-to-race flow.
- Sharing a lobby with friends is primarily done by a canonical URL that opens the correct room directly.
- The host can select the game language, and that selection drives both UI localization and the Wikipedia language edition used for gameplay.
- The lobby host can set source and destination pages either by resolved search or curated randomization.
- The randomization flow avoids obscure pages through a documented server-side eligibility heuristic.
- Players can see the current reported pages being browsed by other players during a multiplayer match.
- The in-game UI shows the article content, elapsed time, other players’ current pages, a table of contents, and hop counts for each player.
- When a player reaches the destination, that finish is shown to everyone while remaining players can continue or abandon.
- Wikipedia content is rendered in an application-controlled viewer rather than relying on a raw embedded Wikipedia page.
- The server coordinates match state and finish detection without performing link-by-link server-side navigation validation.
- The solution follows a layered scalable architecture.
- Test coverage exists across domain, integration, and end-to-end layers for critical flows.
- Reconnection and cleanup behaviors are implemented.
- Observability and security baselines are present.

## 22. Recommended First Implementation Decisions

The following decisions are recommended to reduce future rework:

- Start with a modular monolith, not microservices.
- Treat each match as an isolated concurrency boundary.
- Define WebSocket contracts before building UI screens.
- Abstract Wikipedia access behind an application-facing interface from day one.
- Keep manual article search and curated random article selection behind separate backend interfaces, even if the first provider implements both.
- Add architecture tests early to prevent boundary erosion.
- Build reconnect and idempotency into the first protocol iteration, not later.

## 23. Open Design Questions

These questions should be resolved before implementation begins:

- Which Wikipedia source strategy should be used first: live API, cached API, or preprocessed dump-backed provider?
- Which weighted signals should define the initial well-known article eligibility score for randomization?
- Should random start and target generation rely on curated difficulty tiers?
- Should spectators be included in version 1 UI or only supported at protocol level?
- Which Angular state management approach best matches the team’s long-term preferences?
- What level of anti-cheat telemetry is required in the first release?

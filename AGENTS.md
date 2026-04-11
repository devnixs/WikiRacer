# AGENTS.md

## Project Summary

WikiRacer is a production-oriented Wikiracer clone built as a monorepo with:

- A .NET 10 backend under `backend/`
- An Angular 21 frontend under `frontend/`
- Shared transport contracts in `backend/src/WikiRacer.Contracts`
- Story-driven planning and specifications in `stories/`, `SPECS.md`, and `docs/`

Core product behavior today:

- Players create and join private lobbies
- Lobby state is synchronized in real time over WebSockets
- Hosts configure language, start article, and target article
- Matches can be started, progressed, finished, and abandoned
- All runtime state is in memory only
- No persistent database exists in v1

## Repository Layout

- `README.md`: high-level setup
- `SPECS.md`: long-form product and architecture specification
- `stories/`: implementation backlog and story-level intent
- `docs/`: contributor docs and protocol notes
- `ops/`: operations placeholder material
- `backend/`: .NET solution projects and tests
- `frontend/`: Angular application

Important backend projects:

- `backend/src/WikiRacer.Api`: HTTP API, WebSocket endpoint registration, DI composition
- `backend/src/WikiRacer.Application`: use-case orchestration and application abstractions
- `backend/src/WikiRacer.Domain`: domain model and rules
- `backend/src/WikiRacer.Infrastructure`: in-memory repositories and Wikipedia/runtime adapters
- `backend/src/WikiRacer.Contracts`: API and WebSocket DTOs/contracts
- `backend/tests/WikiRacer.ArchitectureTests`: architecture and service tests

Important frontend areas:

- `frontend/src/app/features/home`: landing page
- `frontend/src/app/features/lobby`: lobby UI, HTTP client, realtime client, session persistence
- `frontend/src/app/features/match`: match UI, article rendering, race APIs, solo session persistence
- `frontend/src/app/features/results`: results page
- `frontend/src/app/shared`: reusable layout and UI components
- `frontend/src/styles*`: global SCSS theme and component styles

## Toolchain And Versions

- .NET SDK pinned by `global.json` to `10.0.102`
- Backend target framework: `net10.0`
- Node.js: `24.x` recommended
- npm: `11.x`
- Angular: `21.x`
- TypeScript: `~5.9.2`
- Tests: xUnit on backend, Angular unit-test builder on frontend, Vitest present in dependencies

## How To Run

From repo root:

```bash
dotnet restore WikiRacer.slnx
dotnet build WikiRacer.slnx
dotnet test WikiRacer.slnx
dotnet run --project backend/src/WikiRacer.Api
```

Frontend:

```bash
cd frontend
npm install
npm start
```

Useful dev variants:

```bash
dotnet watch --project backend/src/WikiRacer.Api run
cd frontend && npm run build
cd frontend && npm test
```

## Runtime Topology

Frontend dev server:

- Runs via `ng serve --proxy-config proxy.conf.json`
- Proxies `/api` and `/ws` to `http://localhost:5295`

Backend API:

- Hosts REST controllers plus a WebSocket endpoint
- Exposes `/health`
- Uses CORS policy named `frontend` for `http://localhost:4200`
- Uses in-memory stores for lobbies, matches, and player sessions

WebSocket endpoint:

- `/ws/lobbies/{publicLobbyId}?reconnectToken=...`
- Registered in `backend/src/WikiRacer.Api/Lobbies/LobbyRealtimeEndpoints.cs`

## Current HTTP API Surface

Lobby routes in `backend/src/WikiRacer.Api/Controllers/LobbiesController.cs`:

- `POST /api/lobbies`
- `GET /api/lobbies/{publicLobbyId}`
- `POST /api/lobbies/{publicLobbyId}/join`
- `PUT /api/lobbies/{publicLobbyId}/language`
- `PUT /api/lobbies/{publicLobbyId}/players/{playerId}/ready`
- `POST /api/lobbies/{publicLobbyId}/match/start`
- `GET /api/lobbies/{publicLobbyId}/match`

Article routes in `backend/src/WikiRacer.Api/Controllers/ArticlesController.cs`:

- `GET /api/articles/search`
- `GET /api/articles/render`
- `PUT /api/lobbies/{publicLobbyId}/articles/{slot}`
- `POST /api/lobbies/{publicLobbyId}/articles/{slot}/randomize`

Match routes in `backend/src/WikiRacer.Api/Controllers/MatchesController.cs`:

- `POST /api/matches/{matchId}/progress`
- `POST /api/matches/{matchId}/abandon`

Health route:

- `GET /health`

## Architecture Rules

These rules are enforced by tests in `backend/tests/WikiRacer.ArchitectureTests/ArchitectureDependencyTests.cs`.

- `WikiRacer.Domain` must not reference API, infrastructure, or contracts
- `WikiRacer.Application` may depend on domain, not on API/infrastructure/contracts
- `WikiRacer.Contracts` must not reference other project assemblies
- `WikiRacer.Infrastructure` may depend on application and domain, not API/contracts
- `WikiRacer.Api` depends on application, infrastructure, and contracts, but not domain directly

If you change project references, update with caution. The tests will catch forbidden coupling.

## Backend Design Notes

Composition root:

- `backend/src/WikiRacer.Api/Program.cs`
- Registers controllers, JSON enum serialization, CORS, WebSockets, HTTP client, repositories, services, and adapters

Mapping helpers:

- `backend/src/WikiRacer.Api/Lobbies/LobbyMappings.cs`
- `backend/src/WikiRacer.Api/Matches/MatchMappings.cs`

Error handling:

- Controller base class is `backend/src/WikiRacer.Api/Controllers/ApiControllerBase.cs`
- Domain/application operation failures are surfaced through `LobbyOperationException`
- API error payload shape lives in `backend/src/WikiRacer.Contracts/Errors/ErrorPayload.cs`

Application layer hotspots:

- `LobbyService`: create/join/get/update language/update readiness
- `MatchService`: start/get/progress/abandon
- `WikipediaArticleService`: search/render/update/randomize article selection

Infrastructure implementations:

- `InMemoryLobbyRepository`
- `InMemoryMatchRepository`
- `InMemoryPlayerSessionStore`
- `WikipediaArticleClient`
- `CuratedPlayableArticleSelector`
- `RecognizableArticleEligibilityScorer`
- `SessionTokenFactory`
- `PublicLobbyIdGenerator`
- `SystemClock`

Important project constraint:

- Runtime state is ephemeral. Restarts wipe lobbies, matches, sessions, and reconnect state.

## Frontend Design Notes

Routing is declared in `frontend/src/app/app.routes.ts`:

- `/`
- `/lobby`
- `/lobby/:publicLobbyId`
- `/match`
- `/match/:publicLobbyId`
- `/results`

Frontend service split:

- `lobby-api.service.ts`: REST calls for lobby and article-selection actions
- `lobby-realtime.service.ts`: WebSocket connection for lobby snapshots and updates
- `lobby-session.service.ts`: `sessionStorage` for reconnect/session tokens
- `match-api.service.ts`: match start/progress/abandon HTTP calls
- `article-render.service.ts`: render article HTML from backend
- `solo-race-session.service.ts`: local solo race persistence in `sessionStorage`

Realtime behavior:

- Frontend currently reacts mainly to `lobby.snapshot` and `lobby.updated`
- Message envelopes follow the protocol documented in `docs/websocket-protocol.md`

Important frontend constraint:

- Session and solo race state are browser-session scoped, not durable across restarts or browser storage clearing

## Testing And Validation

Backend:

- `dotnet test WikiRacer.slnx`
- Architecture tests cover dependency boundaries
- Service tests cover lobby and match behavior
- Serialization and Wikipedia article service tests exist

Frontend:

- `cd frontend && npm test`
- `cd frontend && npm run build`

CI:

- `.github/workflows/ci.yml`
- Builds backend in Release mode
- Builds frontend after `npm ci`
- CI currently does not run backend tests in the workflow

## Docs Worth Reading Before Larger Changes

- `SPECS.md`: product intent and long-term constraints
- `docs/CONTRIBUTING.md`: workflow and layering expectations
- `docs/websocket-protocol.md`: envelope/versioning/replay semantics
- `stories/README.md`: story organization
- Relevant `stories/*.md`: implementation-level intent for the area you are changing

## Practical Change Guidance

- Keep transport contracts in `WikiRacer.Contracts`, not in API or frontend-only models
- Keep mapping logic in the API layer, not in application/domain
- Avoid adding direct `WikiRacer.Domain` references to `WikiRacer.Api`
- Preserve the in-memory-first design unless the task explicitly introduces persistence
- If you add API endpoints, prefer controllers for HTTP and dedicated endpoint registration files for non-controller routes like WebSockets
- If you change WebSocket payloads or DTOs, update both `WikiRacer.Contracts` and the frontend realtime consumers
- If you change public API shapes, check frontend service models for drift
- If you change project boundaries, run the architecture tests
- If you touch article selection or rendering, inspect both backend service code and the frontend match/lobby callers

## Known Operational Assumptions

- Default frontend origin expected by the backend is `http://localhost:4200`
- Frontend proxy expects backend on `http://localhost:5295`
- Health check exists at `/health`
- User agent for outbound Wikipedia HTTP calls is set in `Program.cs`

## If You Need A Fast Mental Model

Think of the system as:

1. Angular pages and services call REST endpoints for create/join/configure/start/progress actions.
2. The backend application layer updates in-memory lobby and match state.
3. The API broadcasts lobby and match changes over WebSockets through `LobbyRealtimeHub`.
4. Contracts in `WikiRacer.Contracts` define the wire format between backend and frontend.
5. Specs and stories define what behavior should exist, even when the current implementation is still incomplete.

# WikiRacer

Production-oriented Wikiracer clone built as a monorepo with a .NET 10 backend and an Angular frontend.

## Repository Layout

- `backend/` contains the .NET solution projects and backend-specific notes.
- `frontend/` contains the Angular workspace and client application shell.
- `docs/` stores project and contributor documentation.
- `ops/` stores deployment and operations placeholders for later stories.
- `stories/` contains the implementation backlog split into ordered stories.

## Local Setup

### Prerequisites

- .NET SDK `10.0.102` or newer in the .NET 10 line
- Node.js `24.x` LTS recommended
- npm `11.x`

### Backend

```bash
dotnet restore WikiRacer.slnx
dotnet build WikiRacer.slnx
dotnet test WikiRacer.slnx
dotnet run --project backend/src/WikiRacer.Api
```

The API exposes a minimal foundation endpoint at `/health`.

### Frontend

```bash
cd frontend
npm install
npm start
```

The Angular app runs on the default dev-server port and currently provides a routed shell for the home, lobby, match, and results areas.

## Quality Baseline

- Root `.editorconfig` establishes shared whitespace and encoding rules.
- Frontend formatting uses Prettier via the Angular workspace config.
- CI placeholder workflow builds both backend and frontend on every push and pull request.

Additional contributor notes live in [docs/CONTRIBUTING.md](/Users/raphael/Projects/CSharp/WikiRacer/docs/CONTRIBUTING.md).

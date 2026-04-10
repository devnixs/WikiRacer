# Contributing

## Development Workflow

1. Read the relevant story in `stories/` before changing implementation details.
2. Keep backend layering intact:
   - `WikiRacer.Domain` stays free of infrastructure and transport concerns.
   - `WikiRacer.Application` depends on domain logic.
   - `WikiRacer.Infrastructure` implements application-facing capabilities.
   - `WikiRacer.Api` hosts HTTP and WebSocket entry points.
   - `WikiRacer.Contracts` holds shared transport contracts.
3. Keep frontend work inside the Angular workspace under `frontend/`.

## Useful Commands

### Backend

```bash
dotnet restore WikiRacer.slnx
dotnet build WikiRacer.slnx
dotnet test WikiRacer.slnx
dotnet watch --project backend/src/WikiRacer.Api run
```

### Frontend

```bash
cd frontend
npm install
npm start
npm run build
```

## Notes

- Node.js `24.x` LTS is the recommended local runtime even if newer odd-numbered releases can work during development.
- `frontend/node_modules` and build artifacts are intentionally ignored at the repository root.
- CI currently validates that the foundation builds; deeper checks land in later stories.

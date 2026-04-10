# Backend

The backend is organized as a layered .NET 10 solution under `backend/src`.

- `WikiRacer.Api` hosts transport and composition concerns.
- `WikiRacer.Application` holds use-case orchestration.
- `WikiRacer.Domain` holds core domain rules and types.
- `WikiRacer.Infrastructure` holds external integrations and runtime adapters.
- `WikiRacer.Contracts` holds shared transport contracts.

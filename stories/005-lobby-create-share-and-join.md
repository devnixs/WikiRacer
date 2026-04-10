# Story 005: Lobby Create Share And Join

## Goal

Implement URL-first lobby creation and joining.

## Scope

- Create lobbies in memory.
- Generate public-safe shareable lobby identifiers.
- Support route-based join flow from a canonical URL.
- Handle invalid, expired, or full lobbies gracefully.

## Acceptance Criteria

- A user can create a lobby and receive a shareable URL.
- Opening the URL enters the correct join flow.
- The UI exposes a copyable share link.
- Join failure states are handled clearly.

## Dependencies

- [002-backend-core-architecture.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/002-backend-core-architecture.md)
- [003-contracts-and-websocket-protocol.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/003-contracts-and-websocket-protocol.md)
- [004-angular-workspace-and-app-shell.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/004-angular-workspace-and-app-shell.md)

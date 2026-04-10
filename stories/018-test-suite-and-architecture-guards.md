# Story 018: Test Suite And Architecture Guards

## Goal

Lock in quality with automated tests and boundary enforcement.

## Scope

- Add unit tests for domain and application rules.
- Add integration tests for WebSocket flows.
- Add frontend component and end-to-end coverage for core user journeys.
- Enforce architecture boundaries in CI.

## Acceptance Criteria

- Core backend domain rules have unit test coverage.
- Critical WebSocket flows have integration coverage.
- Frontend core journeys have automated coverage.
- Architecture tests fail when forbidden dependencies are introduced.

## Dependencies

- [002-backend-core-architecture.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/002-backend-core-architecture.md)
- [003-contracts-and-websocket-protocol.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/003-contracts-and-websocket-protocol.md)
- [004-angular-workspace-and-app-shell.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/004-angular-workspace-and-app-shell.md)

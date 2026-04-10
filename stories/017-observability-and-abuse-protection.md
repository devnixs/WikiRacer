# Story 017: Observability And Abuse Protection

## Goal

Add production-grade diagnostics and baseline abuse controls.

## Scope

- Add structured logging with match, lobby, player, and connection identifiers.
- Add metrics for active state, transitions, and reconnect outcomes.
- Add request and message size limits.
- Add per-connection rate limiting and deduplication safeguards.

## Acceptance Criteria

- Logs contain correlation context for core flows.
- Metrics exist for active matches, active connections, and reported page transitions.
- Oversized or abusive traffic is rejected safely.
- WebSocket message deduplication is measurable and testable.

## Dependencies

- [012-multiplayer-match-orchestration.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/012-multiplayer-match-orchestration.md)
- [015-reconnection-and-resync.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/015-reconnection-and-resync.md)

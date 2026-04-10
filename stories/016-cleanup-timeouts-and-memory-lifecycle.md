# Story 016: Cleanup Timeouts And Memory Lifecycle

## Goal

Control memory growth and remove stale runtime state safely.

## Scope

- Expire empty lobbies.
- Expire finished matches after a retention window.
- Expire reconnect tokens and disconnected sessions.
- Add hosted cleanup jobs with observability.

## Acceptance Criteria

- Stale lobbies and matches are evicted according to configuration.
- Cleanup does not remove active sessions incorrectly.
- Cleanup activity is logged and measurable.
- In-memory retention settings are externally configurable.

## Dependencies

- [012-multiplayer-match-orchestration.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/012-multiplayer-match-orchestration.md)
- [015-reconnection-and-resync.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/015-reconnection-and-resync.md)

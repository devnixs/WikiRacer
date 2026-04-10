# Story 015: Reconnection And Resync

## Goal

Allow a disconnected player to resume an in-memory lobby or match.

## Scope

- Add transient reconnect identity and session rebinding.
- Support resync from a snapshot plus recent updates.
- Restore lobby or match presence after reconnect.
- Handle stale or expired reconnection attempts.

## Acceptance Criteria

- A temporarily disconnected player can reconnect to an active match.
- The client receives enough state to resume correctly.
- Stale reconnect attempts are rejected safely.
- Reconnection state is visible and consistent for other players.

## Dependencies

- [011-lobby-realtime-sync.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/011-lobby-realtime-sync.md)
- [012-multiplayer-match-orchestration.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/012-multiplayer-match-orchestration.md)

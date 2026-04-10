# Story 011: Lobby Realtime Sync

## Goal

Keep lobby state synchronized across all participants through WebSockets.

## Scope

- Broadcast lobby membership changes.
- Broadcast language, source, and destination changes.
- Broadcast readiness and countdown state.
- Keep frontend state synchronized from server snapshots and updates.

## Acceptance Criteria

- Multiple users in the same lobby see shared state updates in real time.
- Host changes appear to all participants without manual refresh.
- Lobby snapshots and incremental updates are both supported.
- Countdown transitions are reflected consistently across clients.

## Dependencies

- [003-contracts-and-websocket-protocol.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/003-contracts-and-websocket-protocol.md)
- [005-lobby-create-share-and-join.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/005-lobby-create-share-and-join.md)

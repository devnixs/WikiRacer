# Story 012: Multiplayer Match Orchestration

## Goal

Implement match lifecycle orchestration for multiplayer races.

## Scope

- Start a multiplayer match from an active lobby.
- Maintain match-level state and per-player race status.
- Track reported page transitions and hop counts for each player.
- Produce final rankings and finish times.

## Acceptance Criteria

- A lobby can transition into a multiplayer match.
- Each player has explicit race status such as active, finished, abandoned, or disconnected.
- The server tracks current reported page and hop count per player.
- Final results can be computed in memory for the match lifecycle.

## Dependencies

- [010-solo-race-loop.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/010-solo-race-loop.md)
- [011-lobby-realtime-sync.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/011-lobby-realtime-sync.md)

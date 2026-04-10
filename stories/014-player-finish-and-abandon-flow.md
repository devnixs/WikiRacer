# Story 014: Player Finish And Abandon Flow

## Goal

Support first-finisher announcements while allowing remaining players to continue or abandon.

## Scope

- Broadcast player-finished events.
- Broadcast player-abandoned events.
- Keep the match active after the first finisher.
- Reflect finish and abandon states in the shared UI.

## Acceptance Criteria

- When a player reaches the destination, all participants see the finish announcement.
- Remaining players can keep racing for placement.
- Remaining players can explicitly abandon.
- The match ends only when the configured completion condition is met.

## Dependencies

- [012-multiplayer-match-orchestration.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/012-multiplayer-match-orchestration.md)
- [013-match-screen-layout-and-race-status.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/013-match-screen-layout-and-race-status.md)

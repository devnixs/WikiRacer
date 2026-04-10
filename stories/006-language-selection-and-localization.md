# Story 006: Language Selection And Localization

## Goal

Make the selected game language drive both UI localization and Wikipedia content selection.

## Scope

- Add lobby language selection.
- Propagate selected language through contracts and runtime state.
- Add Angular localization support for supported game languages.
- Invalidate or re-resolve selected pages when the language changes.

## Acceptance Criteria

- The host can choose the game language in the lobby.
- UI strings update according to the selected language.
- Search and article retrieval use the selected Wikipedia edition.
- Existing source and destination selections are handled correctly on language change.

## Dependencies

- [005-lobby-create-share-and-join.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/005-lobby-create-share-and-join.md)

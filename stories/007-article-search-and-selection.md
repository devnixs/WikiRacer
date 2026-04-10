# Story 007: Article Search And Selection

## Goal

Allow the host to choose source and destination pages manually using search.

## Scope

- Integrate Wikipedia title search.
- Build searchable source and destination pickers.
- Resolve and normalize selected titles.
- Reject unsupported namespaces and non-playable pages.

## Acceptance Criteria

- The host can search and choose source and destination pages.
- Search suggestions are language-scoped.
- The backend resolves canonical page titles before storing lobby state.
- The selected pages are broadcast to other lobby participants.

## Dependencies

- [005-lobby-create-share-and-join.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/005-lobby-create-share-and-join.md)
- [006-language-selection-and-localization.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/006-language-selection-and-localization.md)

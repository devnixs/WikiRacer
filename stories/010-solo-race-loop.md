# Story 010: Solo Race Loop

## Goal

Implement the end-to-end solo gameplay flow.

## Scope

- Start a solo match from selected source and destination pages.
- Track the current reported page and hop count.
- Update elapsed time during play.
- Detect finish against the normalized destination page.

## Acceptance Criteria

- A player can start and complete a solo race.
- Reported page transitions update the current page and hop count.
- The timer runs correctly during the match.
- Reaching the destination produces a finished result.

## Dependencies

- [007-article-search-and-selection.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/007-article-search-and-selection.md)
- [008-curated-random-article-selection.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/008-curated-random-article-selection.md)
- [009-article-rendering-pipeline.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/009-article-rendering-pipeline.md)

# Story 008: Curated Random Article Selection

## Goal

Provide a `Randomize` flow that favors well-known Wikipedia pages instead of obscure ones.

## Scope

- Create the `IPlayableArticleSelector` abstraction.
- Define an eligibility scoring model for recognizable pages.
- Implement random source and destination selection.
- Reroll identical or obviously poor random pairs.

## Acceptance Criteria

- The host can randomize source and destination independently.
- Randomization draws from a curated eligibility pool.
- The scoring strategy is encapsulated behind an interface.
- The system avoids identical source and destination pages after normalization.

## Dependencies

- [007-article-search-and-selection.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/007-article-search-and-selection.md)

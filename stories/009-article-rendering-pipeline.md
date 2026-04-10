# Story 009: Article Rendering Pipeline

## Goal

Render Wikipedia content inside the app through an application-controlled viewer.

## Scope

- Fetch article HTML through official Wikimedia or MediaWiki APIs.
- Sanitize renderable content.
- Rewrite internal article links into app-controlled navigation.
- Preserve attribution and a link back to the source article.

## Acceptance Criteria

- Article content renders in-app without a raw Wikipedia iframe.
- Internal article links stay inside the game UI.
- External links are handled safely.
- Attribution and source links are shown clearly.

## Dependencies

- [004-angular-workspace-and-app-shell.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/004-angular-workspace-and-app-shell.md)
- [006-language-selection-and-localization.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/006-language-selection-and-localization.md)

# Story 002: Backend Core Architecture

## Goal

Establish the layered backend architecture and project boundaries.

## Scope

- Create `WikiRacer.Api`, `WikiRacer.Application`, `WikiRacer.Domain`, `WikiRacer.Infrastructure`, and `WikiRacer.Contracts`.
- Define dependency direction between projects.
- Add baseline domain primitives and identifier value objects.
- Add architecture test scaffolding.

## Acceptance Criteria

- Project references follow the intended layered architecture.
- Domain code has no transport or infrastructure dependencies.
- Core identifiers and shared domain primitives are defined.
- Architecture tests can enforce forbidden references.

## Dependencies

- [001-foundation-repository-and-solution.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/001-foundation-repository-and-solution.md)

# Story 003: Contracts And WebSocket Protocol

## Goal

Define the HTTP and WebSocket contracts before feature implementation expands.

## Scope

- Create versioned message envelopes.
- Define lobby, match, progress, finish, abandon, and reconnect message types.
- Define shared error codes and error payloads.
- Document protocol expectations for deduplication and resync.

## Acceptance Criteria

- Client-to-server and server-to-client contracts are versioned.
- Core match messages are represented in shared contracts.
- Serialization tests exist for representative messages.
- Error responses use stable machine-readable codes.

## Dependencies

- [002-backend-core-architecture.md](/Users/raphael/Projects/CSharp/WikiRacer/stories/002-backend-core-architecture.md)

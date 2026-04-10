# WebSocket Protocol

## Versioning

- All client-to-server and server-to-client envelopes carry a `version` field.
- Story 003 defines protocol version `1.0`.
- Message handlers must reject unsupported protocol versions with a stable machine-readable error code.

## Envelope Shapes

### Client To Server

```json
{
  "version": "1.0",
  "messageType": "match.progress.report",
  "messageId": "msg-001",
  "sentAtUtc": "2026-04-10T12:00:00+00:00",
  "payload": {}
}
```

### Server To Client

```json
{
  "version": "1.0",
  "messageType": "match.player.progressed",
  "sequence": 42,
  "sentAtUtc": "2026-04-10T12:00:01+00:00",
  "correlationId": "msg-001",
  "payload": {}
}
```

## Deduplication Rules

- `messageId` is client-generated and must be unique within the active client session.
- Servers should treat a repeated `messageId` for the same session as a duplicate command and avoid applying it twice.
- Duplicate commands should be ignored or answered with `duplicate_message`, depending on handler semantics.
- `correlationId` lets the server point a response or rejection back to the originating client message.

## Ordering And Resync

- `sequence` is server-generated and monotonic within a lobby or match event stream.
- Clients should track the last applied `sequence` per active scope.
- Reconnect and resync requests should provide the last received sequence when available.
- If the server can replay from that sequence, it may send the missing events.
- If replay is not possible or the sequence is stale, the server should emit `session.resync.required` and then provide a fresh snapshot.
- Snapshot events (`lobby.snapshot`, `match.snapshot`) are the authoritative restart point after reconnect.

## Core Message Families

- Lobby commands and events cover join, leave, readiness, and settings synchronization.
- Match commands and events cover start, progress, finish, and abandon flows.
- Session commands and events cover reconnect acceptance and forced resync.
- Error events use stable machine-readable codes defined in `WikiRacer.Contracts.Errors.ErrorCodes`.

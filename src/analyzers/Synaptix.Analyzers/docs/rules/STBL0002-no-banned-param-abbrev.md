# STBL0002 — No banned parameter abbreviations

## Rationale

Per [`naming-and-types.md`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): parameter names must be descriptive full names. Abbreviations like `ct` (cancellationToken), `req` (request), `resp` (response), `err` (error), `msg` (message), `ex` (exception), `svc` (service), `u`/`x`/`tmp` reduce readability and obscure intent.

## Examples

❌ `void Process(string ct, OrderRequest req)`
✅ `void Process(string cancellationToken, OrderRequest request)`

❌ `Task SendAsync(IMessage msg, CancellationToken ct)`
✅ `Task SendAsync(IMessage message, CancellationToken cancellationToken)`

## Banned abbreviations

`ct`, `req`, `resp`, `err`, `msg`, `ex`, `svc`, `u`, `x`, `tmp`.

## CodeFix

None in v1.0 — rename is a project-wide decision (collateral renames of call sites, expression-bound identifiers).

## Configuration

Severity via `.editorconfig`:
```ini
dotnet_diagnostic.SYNAPTIX0002.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0002.severity = error  # promote to error
```

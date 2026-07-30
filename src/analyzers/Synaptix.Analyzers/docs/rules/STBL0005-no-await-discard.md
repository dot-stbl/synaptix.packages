# STBL0005 — No `_ = await ...` discard

## Rationale

Per [`async-and-tasks.md §6`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): `_ =` is a misuse of the discard pattern. `await` already waits synchronously — discarding the result does nothing. Worse, the visual signal says "fire-and-forget" while the code is doing the opposite, confusing the next reader.

The fix is to drop the `_ =`:

```csharp
// ❌ Wrong — discard over await
_ = await db.SaveChangesAsync(ct);

// ✅ Correct — bare await
await db.SaveChangesAsync(ct);
```

For genuine fire-and-forget (rare; usually a sign of a design issue), document it explicitly with a comment so the next reader knows it's intentional:

```csharp
// ✅ Genuine fire-and-forget (with justification)
_ = telemetry.FlushAsync(ct);  // fire-and-forget; not critical if the buffer is lost
```

## Examples

❌ `_ = await db.Users.AddAsync(user, ct);`
✅ `await db.Users.AddAsync(user, ct);`

❌ `_ = await transaction.CommitAsync(ct);`
✅ `await transaction.CommitAsync(ct);`

✅ `_ = SomeFuncReturningTask().ContinueWith(...);` — discard on a non-await, with a comment explaining the fire-and-forget intent.

## CodeFix

Title: "Remove discard". Replaces `_ = await foo();` with `await foo();`. Applied via `WellKnownFixAllProviders.BatchFixer`.

## Configuration

Severity via `.editorconfig`:
```ini
dotnet_diagnostic.SYNAPTIX0005.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0005.severity = error  # promote to error
```

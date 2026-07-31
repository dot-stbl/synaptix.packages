# STBL0011 — No `catch (Exception)` outside top-level handler

## Rationale

Per [`exceptions.md §2`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): a catch-all (`catch (Exception)` or bare `catch`) is allowed **only** in the single composition-root handler that maps exceptions to `ProblemDetails`. Everywhere else, catch the specific exception type you can act on. A catch-all hides the boundary contract, swallows bugs, and forces every consumer to guess what could go wrong.

```csharp
// ❌ Catch-all hides what's expected to fail
public Order GetOrder(int id)
{
    try { return _repo.Find(id); }
    catch (Exception) { return null; }   // swallowed bug
}

// ✅ Narrow — translate the one thing this layer expects
public Order GetOrder(int id)
{
    try { return _repo.Find(id); }
    catch (RepositoryNotFoundException ex)
    {
        throw new OrderNotFoundException(id, ex);
    }
}
```

A bare `catch` (no type) is **semantically** `catch (Exception)` — both forms are flagged.

## Reserved exception types

Only three types are treated as catch-alls:

- `System.Exception`
- `System.ApplicationException`
- `System.SystemException`

`System.ApplicationException` and `System.SystemException` are legacy base classes that, like `System.Exception`, capture every standard CLR exception. Specific subclasses (`ArgumentException`, `HttpRequestException`, `InvalidOperationException`, …) are **not** catch-alls and are NOT flagged — catch what you can act on.

## Skip conditions

The analyzer does **not** fire when the enclosing method has:

- `[Obsolete]` — the call site has been told not to use it; an opt-out is cheap.
- `[TopLevelExceptionHandler]` (or `[TopLevelExceptionHandlerAttribute]`) — opt-in marker for the composition-root handler(s). The attribute itself is reserved for later definition; in v1.0 the analyzer matches by simple name so users can declare a local marker.

The analyzer also does **not** fire when the catch body is a bare rethrow:

```csharp
catch (Exception) { throw; }   // passes through — fine at any level
```

## CodeFix

None — the correct replacement (which specific exception to catch, or which domain exception to translate to) is a design decision that depends on the caller's contract. The IDE surfaces a lightbulb with a manual edit.

## Configuration

Severity via `.editorconfig`:

```ini
dotnet_diagnostic.SYNAPTIX0011.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0011.severity = error  # promote to error
```
# STBL0004 — No `ArgumentNullException.ThrowIfNull*` in non-boundary code

## Rationale

Per [`code-shape.md §11`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): `<Nullable>enable</Nullable>` is global. When a parameter is declared non-nullable (`string x`, not `string? x`), the compiler enforces non-null at every call site. A runtime `ArgumentNullException.ThrowIfNull(x)` re-checks what the compiler already guarantees, lies to the reader ("this method may receive null"), and adds noise.

Trust the signature. Replace with pattern matching when a real check is needed:

```csharp
// ❌ Redundant — compiler already rejected null
public void Process(string name)
{
    ArgumentNullException.ThrowIfNull(name);
    // ...
}

// ✅ Same effect, no runtime check
public void Process(string name)
{
    // ... name is non-null by signature
}
```

For cases where the value *can* be null at runtime (e.g. reading from a dictionary), prefer a domain exception with a stable `Code`:

```csharp
// ✅ When the contract really is "may be null"
return dictionary.TryGetValue(key, out var value)
    ? value
    : throw new NotFoundException("key.not_found", $"key '{key}' was not found");
```

## Boundary exception

Reflection, interop (`P/Invoke`), and deserialisation are nullable-oblivious — the compiler can't enforce. In those places, add a `// boundary:` comment and the analyzer will skip the method (TODO in v1.0; v1.0 skips methods annotated with `[Obsolete]` or `[ExcludeFromCodeCoverage]` as a heuristic).

## Examples

❌ `ArgumentNullException.ThrowIfNull(name)`
❌ `ArgumentNullException.ThrowIfNullOrEmpty(s)`
❌ `ArgumentNullException.ThrowIfNullOrWhiteSpace(s)`
❌ `ArgumentException.ThrowIfNullOrEmpty(s)`
❌ `ArgumentException.ThrowIfNullOrWhiteSpace(s)`

## CodeFix

None in v1.0 — the correct replacement depends on context (drop the check, use pattern matching, throw a domain exception).

## Configuration

Severity via `.editorconfig`:
```ini
dotnet_diagnostic.SYNAPTIX0004.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0004.severity = error  # promote to error
```

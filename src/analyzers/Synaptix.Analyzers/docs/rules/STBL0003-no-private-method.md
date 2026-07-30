# STBL0003 — No private methods in production classes

## Rationale

Per [`class-layout-and-tooling.md §1a`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): production classes orchestrate; helpers live in their own files. A `private` method on a class is procedural style — it ties logic to one consumer, hides it from review and tests, and resists isolated unit testing.

The fix is mechanical:

| Original | Extracted |
|----------|-----------|
| `private static int Compute(int x) => x * 2;` | `internal static class FooHelpers { public static int Compute(int x) => x * 2; }` |
| `private async Task<Bar> LoadAsync(...)` (with deps) | `internal sealed class BarLoader(IDep repo) { public Task<Bar> LoadAsync(...) }` injected into the original class |

## Exemptions

The analyzer skips:

- `override` / `virtual` methods.
- `Dispose(bool disposing)` pattern (the `IDisposable` canonical hook).
- Parameterless constructors (EF Core materialisation).
- `private` methods on `abstract` classes (subclasses may need them).
- Members annotated with `[GeneratedCode]` or in `*.Designer.cs` / `*.g.cs` files.

## Examples

❌ `public sealed class OrderProcessor { private void Validate(Order o) { ... } }`
✅ `public sealed class OrderProcessor(Validator validator) { ... }` — `Validator` is a separate injected type.
❌ `public sealed class StringExtensions { private static string Trim(string s) => ...; }` — should be on a `file static class`.
✅ `file static class StringExtensions { public static string Trim(this string s) => ...; }`

## CodeFix

None in v1.0 — extraction is a structural change spanning multiple files; deferred to v1.1.

## Configuration

Severity via `.editorconfig`:
```ini
dotnet_diagnostic.SYNAPTIX0003.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0003.severity = error  # promote to error
```

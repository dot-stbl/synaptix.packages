# STBL0003 — No private methods in production classes

## Rationale

Per [`class-layout-and-tooling.md §1a`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): production classes orchestrate; helpers live in their own files. A `private` method on a class is procedural style — it ties logic to one consumer, hides it from review and tests, and resists isolated unit testing.

## CodeFix

For a private static method, the code fix generates a `{ClassName}Helper` file-scoped static class, available with C# 11+ file-scoped types. The moved method remains a plain static method with its original parameter list:

```csharp
file static class OrderServiceHelper
{
    public static string FormatOrderId(int id) => id.ToString();
}
```

Calls to the moved method inside the original class are rewritten to the fully qualified helper call, such as `FormatOrderId(id)` or `this.FormatOrderId(id)` becoming `OrderServiceHelper.FormatOrderId(id)`. Already-qualified calls such as `other.FormatOrderId(id)` are left untouched. Calls from static methods are also rewritten.

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
❌ `public sealed class StringExtensions { private static string Trim(string s) => ...; }` — should be on a `file static` class.
✅ `file static class StringHelper { public static string Trim(string value) => ...; }``

## Configuration

Severity via `.editorconfig`:
```ini
dotnet_diagnostic.SYNAPTIX0003.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0003.severity = error  # promote to error
```

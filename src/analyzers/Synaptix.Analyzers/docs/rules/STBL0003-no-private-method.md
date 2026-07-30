# STBL0003 — No private methods in production classes

## Rationale

Per [`class-layout-and-tooling.md §1a`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): production classes orchestrate; helpers live in their own files. A `private` method on a class is procedural style — it ties logic to one consumer, hides it from review and tests, and resists isolated unit testing.

## CodeFix

For a private static method, the code fix generates a `{ClassName}Extensions` file-scoped static class, available with C# 11+ file-scoped types. The moved method becomes an extension method by prepending a `this {ClassName} self` parameter:

```csharp
file static class OrderServiceExtensions
{
    public static string FormatOrderId(this OrderService self, int id) => id.ToString();
}
```

Unqualified calls to the moved method inside instance methods of the original class are rewritten to extension syntax, such as `FormatOrderId(id)` becoming `this.FormatOrderId(id)`. Calls in static methods and private instance methods are not offered a fix; instance methods may depend on state that cannot be safely moved automatically.

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
✅ `file static class StringExtensions { public static string Trim(this string s) => ...; }`

## Configuration

Severity via `.editorconfig`:
```ini
dotnet_diagnostic.SYNAPTIX0003.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0003.severity = error  # promote to error
```

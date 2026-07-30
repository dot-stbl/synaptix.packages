# STBL0001 — No banned type-name suffix

## Rationale

Per [`naming-and-types.md`](https://github.com/dot-stbl/synaptix.packages/blob/main/CLAUDE.md): suffixes must describe the *role* of the type, not a generic category. `Dto`, `Model`, `Impl`, `Util`, `Utility`, `ViewModel` describe generic patterns; every Web API class is a "DTO" and every helper is a "util" — the suffix adds no information.

## Examples

❌ `public sealed class UserDto { }`
✅ `public sealed class User { }`
✅ `public sealed class UserSummary { }`   ← projection, has role
✅ `public sealed class UserDetail { }`    ← projection, has role
❌ `public sealed class InvoiceImpl { }`
✅ `public sealed class Invoice { }` or `public sealed class InvoiceService { }`

## Banned suffixes

`Dto`, `Model`, `Impl`, `Util`, `Utility`, `ViewModel`.

## CodeFix

Title: "Strip banned suffix". Removes the suffix and updates all references via `WellKnownFixAllProviders.BatchFixer`.

## Configuration

Severity via `.editorconfig`:
```ini
dotnet_diagnostic.SYNAPTIX0001.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0001.severity = error  # promote to error
```

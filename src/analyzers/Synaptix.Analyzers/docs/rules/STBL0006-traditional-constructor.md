# STBL0006 — Traditional constructor can be replaced with primary constructor

## Rationale

A class with a single constructor whose body only assigns fields from parameters is a candidate for [C# 12 primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#primary-constructors). Primary constructors remove the boilerplate of explicit field declarations and constructor assignments.

## Detection

The analyzer fires when **all** of the following hold for a non-abstract, non-static class that doesn't already have a primary constructor parameter list:

- The class has exactly one constructor.
- The constructor is not static, not decorated with attributes, and has a block body.
- Every statement in the body is a simple assignment of the form `x = e` or `this.x = e`.
- The right-hand side of every assignment is an identifier that appears in the constructor's parameter list.

## CodeFix

The fix rewrites the class to a primary constructor and removes the now-redundant field declarations. Two reconstruction details are worth knowing:

### 1. Both `x = x` and `this.x = x` styles are accepted

The CodeFix recognises both:

```csharp
public OrderService(int x, int y)
{
    x = x;
    this.y = y;
}
```

The diagnostic location is the class identifier; the CodeFix is one per class.

### 2. Underscore-prefixed fields are matched against their non-underscore parameter

If a field is named `_logger` and the constructor has a parameter `logger`, the field is removed and references to `_logger` are rewritten to `logger`. Matching strategies (in order):

1. Exact: `fieldName == paramName`.
2. Parameter has underscore prefix: `fieldName == "_" + paramName`.
3. Field has underscore prefix: `fieldName.TrimStart('_') == paramName`.

If there is no matching parameter, the field is left in place (`_foo` stays when no `foo` parameter exists).

When the field and parameter names differ, **all references** to the field name in the enclosing class are rewritten to the parameter name. The rewriter visits `IdentifierNameSyntax` nodes and skips any nested type declarations, so a local variable named `_logger` inside a nested class is left untouched.

```csharp
// Before
public sealed class OrderService
{
    private readonly string _logger;
    public OrderService(string logger) { _logger = logger; }
    public void Use() { _logger.Info("hi"); }
}

// After (CodeFix)
public sealed class OrderService(string logger)
{
    public void Use() { logger.Info("hi"); }
}
```

Fields with an initializer are preserved (they carry non-constructor state):

```csharp
private readonly int cachedValue = 42;   // stays
```

Multi-variable field declarations are also preserved (the fix only touches single-variable fields).

## Examples

❌
```csharp
public sealed class OrderService
{
    private readonly int x;
    private readonly string y;

    public OrderService(int x, string y)
    {
        this.x = x;
        this.y = y;
    }
}
```
✅
```csharp
public sealed class OrderService(int x, string y)
{
}
```

❌
```csharp
public sealed class OrderService
{
    private readonly string _logger;

    public OrderService(string logger)
    {
        _logger = logger;
    }

    public void Use() { _logger.Info("hi"); }
}
```
✅
```csharp
public sealed class OrderService(string logger)
{
    public void Use() { logger.Info("hi"); }
}
```

## What is NOT flagged

- Bodies with `if` / `throw` / method calls — the analyzer only fires when the body is purely assignment statements.
- Assignments to non-`this` member access (e.g. `inner.Value = x`) — only `this.field` and bare `field` are recognised.
- Assignments with non-identifier right-hand sides (e.g. `this.x = x + 1`).
- Expression-bodied constructors (`=> this.x = x;`).
- Constructors with attributes.
- Multiple constructors or static classes.

## Configuration

Severity via `.editorconfig`:

```ini
dotnet_diagnostic.SYNAPTIX0006.severity = none   # disable
dotnet_diagnostic.SYNAPTIX0006.severity = error  # promote to error
```

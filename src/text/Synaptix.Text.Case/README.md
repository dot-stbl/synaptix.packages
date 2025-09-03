# .NET Case extensions

The library that allows to change any string case to `PascalCase`/`dot.case`/`camelCase`/`kebab-case`/`snake_case`/`Train-Case`.

# Usage

```csharp
using CaseExtensions;

var source = "The Quick Brown Fox";
var result = source.ToDotCase(); // the.quick.brown.fox
```
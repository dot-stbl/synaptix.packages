# Synaptix.Text.Case

Change any string case to `PascalCase` / `camelCase` / `kebab-case` / `snake_case` /
`dot.case` / `Train-Case`.

| Case | `Hello World` |
|------|---------------|
| `PascalCase` | `HelloWorld` |
| `camelCase`  | `helloWorld` |
| `kebab-case` | `hello-world` |
| `snake_case` | `hello_world` |
| `dot.case`   | `hello.world` |
| `Train-Case` | `Hello-World` |

Targets `netstandard2.0`, zero dependencies, no allocations on the hot path.

## Install

```bash
dotnet add package Synaptix.Text.Case
```

## Usage

```csharp
using Synaptix.Text.Case;

"Hello World".ToKebabCase();   // "hello-world"
"some_value".ToPascalCase();   // "SomeValue"
"hello.world".ToTrainCase();   // "Hello-World"
```

The conversions treat spaces, hyphens, underscores, and dots as word
boundaries, and capital letters in the source as word starts:

```csharp
"Hello World".ToKebabCase();         // "hello-world"
"API/users/32".ToCamelCase();       // "api/users/32"
"Some_Action_to_do".ToSnakeCase();   // "some_action_to_do"
```

## Stability

Stable. v1.0+. Used in production by several `.stbl` projects.

## License

GPL-3.0-or-later — see [LICENSE](https://github.com/dot-stbl/synaptix.packages/blob/main/LICENSE).

## Source

[github.com/dot-stbl/synaptix.packages](https://github.com/dot-stbl/synaptix.packages/tree/main/src/text/Synaptix.Text.Case)

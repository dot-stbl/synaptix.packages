# Synaptix Packages

A monorepo of independent .NET libraries and Roslyn analyzers by the
`.stbl` group. Licensed under GPL-3.0-or-later (libraries) and MIT
(analyzers).

## Packages

| Package | Version | License | Description |
|---------|---------|---------|-------------|
| **[Synaptix.Text.Case](src/text/Synaptix.Text.Case/)** | [![NuGet](https://img.shields.io/nuget/v/Synaptix.Text.Case.svg)](https://www.nuget.org/packages/Synaptix.Text.Case) | GPL-3.0 | String case conversions: `PascalCase`, `camelCase`, `kebab-case`, `snake_case`, `dot.case`, `Train-Case` |
| **[Synaptix.Analyzers](src/analyzers/Synaptix.Analyzers/)** | [![NuGet](https://img.shields.io/nuget/v/Synaptix.Analyzers.svg)](https://www.nuget.org/packages/Synaptix.Analyzers) | MIT | Roslyn analyzers encoding `.stbl` C# coding rules. STBL0001–STBL0005 + 2 CodeFixes. |

## Quick Start

### Synaptix.Text.Case

```bash
dotnet add package Synaptix.Text.Case
```

```csharp
using Synaptix.Text.Case;

"Hello World".ToKebabCase();   // "hello-world"
"some_value".ToPascalCase();   // "SomeValue"
```

### Synaptix.Analyzers

```bash
dotnet add package Synaptix.Analyzers
```

Then in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.SYNAPTIX0001.severity = warning
dotnet_diagnostic.SYNAPTIX0002.severity = warning
dotnet_diagnostic.SYNAPTIX0003.severity = warning
dotnet_diagnostic.SYNAPTIX0004.severity = warning
dotnet_diagnostic.SYNAPTIX0005.severity = warning
```

Rules covered: banned type-name suffix, banned parameter abbreviation,
private method in production class, `ArgumentNullException.ThrowIfNull`
in non-boundary code, `_ = await` discard. See
[Synaptix.Analyzers README](src/analyzers/Synaptix.Analyzers/README.md)
for details.

## Build & Test

```bash
git clone https://github.com/dot-stbl/synaptix.packages.git
cd synaptix.packages

# Enable git hooks (strips AI attribution trailers locally)
git config core.hooksPath .githooks

dotnet build synaptix.packages.slnx
dotnet test synaptix.packages.slnx
```

## Versioning

Each package versions independently via git tags + MinVer. Tag prefix
is per-package:

- `v-text-<X.Y.Z>`      → publishes `Synaptix.Text.Case`
- `v-analyzers-<X.Y.Z>`  → publishes `Synaptix.Analyzers`

`release.yml` parses the tag and packs exactly the matching project.
Two tags are needed to release both packages in one go.

## Documentation

- [CONTRIBUTING.md](CONTRIBUTING.md) — workflow, conventions, commit format
- [CLAUDE.md](CLAUDE.md) — architecture and code guidelines
- [AGENTS.md](AGENTS.md) — commands for AI coding agents
- [CHANGELOG.md](CHANGELOG.md) — version history

## License

- **Libraries** (`Synaptix.Text.Case` and future packages): GPL-3.0-or-later — see [LICENSE](LICENSE).
- **Analyzers** (`Synaptix.Analyzers`): MIT — see [src/analyzers/Synaptix.Analyzers/LICENSE](src/analyzers/Synaptix.Analyzers/LICENSE).

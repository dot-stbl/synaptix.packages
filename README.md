# Synaptix Packages

A monorepo of independent .NET libraries by the `.stbl` group. Licensed under
GPL-3.0-or-later.

> **Status:** MVP cleanup. Currently publishing **Synaptix.Text.Case**.
> Other packages removed in this cleanup pass (see CHANGELOG.md for v1.1.0).

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| **[Synaptix.Text.Case](src/text/Synaptix.Text.Case/)** | [![NuGet](https://img.shields.io/nuget/v/Synaptix.Text.Case.svg)](https://www.nuget.org/packages/Synaptix.Text.Case) | String case conversions: `PascalCase`, `camelCase`, `kebab-case`, `snake_case`, `dot.case`, `Train-Case` |

## Quick Start

```bash
# Install
dotnet add package Synaptix.Text.Case

# Use
using Synaptix.Text.Case;

"Hello World".ToKebabCase();   // "hello-world"
"some_value".ToPascalCase();   // "SomeValue"
```

## Build & Test

```bash
git clone https://github.com/dot-stbl/synaptix.packages.git
cd synaptix.packages

# Enable git hooks (strips AI attribution trailers locally)
git config core.hooksPath .githooks

dotnet build synaptix.packages.slnx
dotnet test synaptix.packages.slnx
```

## Documentation

- [CONTRIBUTING.md](CONTRIBUTING.md) — workflow, conventions, commit format
- [CLAUDE.md](CLAUDE.md) — architecture and code guidelines
- [AGENTS.md](AGENTS.md) — commands for AI coding agents
- [CHANGELOG.md](CHANGELOG.md) — version history

## License

GPL-3.0-or-later — see [LICENSE](LICENSE).

# Synaptix.Analyzers

Roslyn analyzers encoding [`.stbl` C# coding rules](https://github.com/dot-stbl/synaptix.packages).

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Install

```bash
dotnet add package Synaptix.Analyzers
```

Then add to your `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.SYNAPTIX0001.severity = warning
dotnet_diagnostic.SYNAPTIX0002.severity = warning
dotnet_diagnostic.SYNAPTIX0003.severity = warning
dotnet_diagnostic.SYNAPTIX0004.severity = warning
dotnet_diagnostic.SYNAPTIX0005.severity = warning
dotnet_diagnostic.SYNAPTIX0006.severity = warning
dotnet_diagnostic.SYNAPTIX0007.severity = warning
dotnet_diagnostic.SYNAPTIX0008.severity = warning
dotnet_diagnostic.SYNAPTIX0009.severity = warning
dotnet_diagnostic.SYNAPTIX0010.severity = warning
```

(IDs are `STBL0001`...`STBL0010`; the analyzer emits them under the `SYNAPTIX` prefix as `SYNAPTIX0001` etc.)

## Rules

| ID | Title | Default Severity | CodeFix | Rationale |
|----|-------|------------------|---------|-----------|
| `STBL0001` | Banned type-name suffix (`Dto`/`Model`/`Impl`/`Util`/`Utility`/`ViewModel`) | Warning | Yes (rename, longest match) | Per `naming-and-types.md` — suffixes must describe role, not generic pattern. |
| `STBL0002` | Banned parameter abbreviation (`ct`/`req`/`resp`/`err`/`msg`/`ex`/`svc`/`u`/`x`/`tmp`) | Warning | Yes (expand) | Per `naming-and-types.md` — full names only. |
| `STBL0003` | Private method in production class | Warning | Yes (extract to file static helper) | Per `class-layout-and-tooling.md §1a` — extract to `file static` helper. |
| `STBL0004` | `ArgumentNullException.ThrowIfNull` in non-boundary code | Warning | Yes (is-null check) | Per `code-shape.md §11` — trust the nullable signature. |
| `STBL0005` | `_ = await ...` discard | Warning | Yes (remove discard) | Per `async-and-tasks.md §6` — bare await or documented fire-and-forget. |
| `STBL0006` | Traditional constructor can be primary constructor (C# 12+) | Warning | Yes (convert) | Per `class-layout-and-tooling.md` — modern C# style. |
| `STBL0007` | Private field uses banned underscore prefix | Warning | Yes (drop prefix) | Per `naming-and-types.md §1` — NO underscore prefix. |
| `STBL0008` | Concrete class is not sealed | Warning | Yes (add `sealed`) | Per `naming-and-types.md §2` — sealed REQUIRED on concrete classes. (Architectural: `sealed` is a structural decision.) |
| `STBL0009` | Collection initializer can be a collection expression (C# 12+) | Warning | Yes (convert) | Per `code-shape.md` — modern C# style. |
| `STBL0010` | `#region` directive is banned | Warning | — (no fix) | Per `code-shape.md §10` — use member organization instead. |

Each rule maps to a specific rule file in this repo:
- [STBL0001](./docs/rules/STBL0001-no-banned-suffixes.md)
- [STBL0002](./docs/rules/STBL0002-no-banned-param-abbrev.md)
- [STBL0003](./docs/rules/STBL0003-no-private-method.md)
- [STBL0004](./docs/rules/STBL0004-no-throw-if-null.md)
- [STBL0005](./docs/rules/STBL0005-no-await-discard.md)
- STBL0006–STBL0010: see [docs/rules/](./docs/rules/) (docs to be expanded as rules stabilize)

## Severity philosophy

- **Warning** for both style and architectural violations. Style issues come with
  CodeFixes that apply silently in your IDE (Ctrl+. → Apply). Architectural issues
  (STBL0003 — extract to `file static`; STBL0010 — no regions) don't have
  CodeFixes because the right answer requires manual context.

## Versioning

Each rule follows [SemVer](https://semver.org/). Breaking change in a rule
(e.g., new banned suffix, lower false-positive tolerance) = **major** bump.
New rule = **minor**. Fix = **patch**.

## License

MIT — see [LICENSE](LICENSE).

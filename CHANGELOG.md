# Changelog

All notable changes to **Synaptix Packages** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Planned
- Additional packages under `src/system/`, `src/extensions/` (if revived).
- npm packages under `src/js/` (bun workspace declared in `package.json`).

## [Synaptix.Analyzers 1.0.0] — 2026-XX-XX

First release of the Roslyn analyzer package. MIT licensed (community-friendly).

### Added (round 1: 5 rules)
- `Synaptix.Analyzers` 1.0.0 — Roslyn analyzer encoding `.stbl` C# coding rules.
- 5 diagnostic rules: `STBL0001` (banned type-name suffix), `STBL0002` (banned parameter abbreviation), `STBL0003` (private method in production class), `STBL0004` (`ArgumentNullException.ThrowIfNull` in non-boundary code), `STBL0005` (`_ = await ...` discard).
- Per-package MinVer tags: `v-analyzers-<X.Y.Z>` → `Synaptix.Analyzers`; `v-text-<X.Y.Z>` → `Synaptix.Text.Case`. Global `<MinVerTagPrefix>v</MinVerTagPrefix>` removed.
- Self-host: `Synaptix.Text.Case` and `Synaptix.Text.Case.Unit` reference the analyzer via `<ProjectReference OutputItemType="Analyzer" />` so the rules fire on this repo's own code.
- Custom in-tree `AnalyzerTestRunner` (8 KB) replaces `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` 1.1.x, which hard-deps Roslyn 1.0.1 and breaks under .NET 8.

### Added (round 2: 5 more rules + 4 CodeFixes)
- **STBL0006**: Traditional constructor can be primary constructor (C# 12+). CodeFix converts and removes the now-redundant field declarations.
- **STBL0007**: Private field uses banned underscore prefix. CodeFix drops the `_` prefix and updates references.
- **STBL0008**: Concrete class is not sealed. CodeFix adds the `sealed` modifier. (Architectural — kept as Warning, no fix isn't possible.)
- **STBL0009**: Collection initializer can be a collection expression (C# 12+). CodeFix converts `new T[] { ... }`, `new List<T> { ... }`, etc. to `[...]` syntax.
- **STBL0010**: `#region` directive is banned. Warning only — no CodeFix (region removal requires manual context).
- **STBL0003 CodeFix** (round 3): extract `private static` method into a `file static class {ClassName}Helpers { public static {Method}(...) }` in the same file. Unqualified call sites within the file are rewritten to the qualified helper name. Skips instance methods, overloads, `[MethodImpl]`-attributed methods, and helper-name collisions. Single-document rewrite via `DocumentEditor` — no cross-file complications. 7 new tests (90 → 97 analyzer tests).
- **STBL0002 CodeFix**: rename banned parameter abbreviation to its expanded form (`ct` → `cancellationToken`, `req` → `request`, etc.).
- **STBL0004 CodeFix**: replace `ArgumentNullException.ThrowIfNull(x)` with `if (x is null) throw new ArgumentNullException(nameof(x));`.
- **STBL0001 fix**: longest-match suffix detection — `AddressViewModel` now reports `ViewModel` rather than `Model`.
- 42 new tests (90 analyzer tests total, +42 from round 1's 48).

### Notes
- `release.yml` rewritten to parse `v-<package>-<version>` tags, build/test only the matching project, pack exactly one `.nupkg`, and push to NuGet.org via Trusted Publishing (OIDC, no API key required). Two tags needed for two packages.
- All 138 tests green (48 text-case + 90 analyzer). `dotnet build` 0 warnings, 0 errors.
- Severity philosophy: style rules with auto-fix surface as Warning + CodeFix (apply silently via Ctrl+. in IDE). Only STBL0010 (`#region`) is Warning only — the right answer needs human judgment. (STBL0003 now has a CodeFix as of round 3.)

## [1.1.0] — 2026-XX-XX

### Changed
- **License corrected**: was `MIT` in `directory.build.props`, now `GPL-3.0-or-later` to match `LICENSE` file.
- **Migration to slnx**: `synaptix.packages.sln` → `synaptix.packages.slnx`.
- **Directory.Build.props unified**: strict mode (TreatWarningsAsErrors + EnforceCodeStyleInBuild), analyzer packages wired (Roslynator, VSTHRD), MinVer for tag-based versioning.
- **Directory.Packages.props** (CPM disabled) added for centralized package versions.
- **`global.json`** pins .NET SDK to 10.0.109 with `rollForward: latestFeature`.
- **`.editorconfig`** strict mode (anlytra-aligned) + RCS1090 (ConfigureAwait) suppressed.
- **`.gitattributes`** enforces LF line endings.
- **`.config/dotnet-tools.json`** local tool manifest for `dotnet-format` + `dotnet-coverage`.
- **Tests migrated from FluentAssertions to Shouldly** (banned globally).
- **Test files renamed**: `*Case.cs` → `*CaseShould.cs` (per `naming-and-types.md`).
- **CI/CD added**: GitHub Actions `ci.yml` (build + test on push/PR), `release.yml` (pack + NuGet push on tag), `lint-commits.yml` (reject AI trailers).
- **Local hooks**: `.githooks/commit-msg` strips AI-attribution trailers before commit.
- **Scripts**: `scripts/format.{sh,ps1}` (verify), `scripts/autofix.{sh,ps1}` (apply).
- **`package.json`** declares bun workspace `src/js/*` for future npm packages.
- **`README.md` rewritten** (was `wip...`).
- **`CONTRIBUTING.md` / `CLAUDE.md` / `AGENTS.md`** added (anlytra-style 4-doc structure).
- **`CODEOWNERS`** uses `@dot-stbl/zero-team`.
- **`dependabot.yml`** weekly NuGet + GitHub Actions updates.
- **Brand submodule** `assets/stbl` from `dot-stbl/.github` (logo, wordmark, lockup, OG templates, by-stbl.css).
- **Package icons unified on canonical stbl parent mark** — both `Synaptix.Text.Case` and `Synaptix.Analyzers` use the same brand mark, rendered from `assets/stbl/assets/logo.svg` at 128×128 and 256×256 → `contents/stbl-128.png` / `contents/stbl-256.png` (B&W, monospace, cut-out quadrant — matches brand guideline).
- **Synaptix-specific gradient icons removed** (`stbl.synaptix.{clr,gzs,gzx,ue,ue.2,wzs}.png`, `stbl.synaptix.png`) and `stbl.database.configuration.png` (orphan, package deleted) — all dropped from `contents/`. `assets/synaptix-packages/` (`synaptix by .stbl` lockup + OG image) also removed — canonical stbl mark replaces it.
- **CI/CD migrated to NuGet Trusted Publishing** (`release.yml`): no long-lived API key, no `NUGET_API_KEY` secret in GitHub. Workflow mints a short-lived token via `NuGet/login@v1` (OIDC, `id-token: write` permission) and exchanges it for a one-shot API key. Trusted Publishing policy must be configured manually on nuget.org (owner: dot-stbl, repo: synaptix.packages, workflow: release.yml, environment: nuget-publish). `CONTRIBUTING.md` troubleshooting updated.

### Removed
- `Synaptix.Color` — only 2 types, dead target frameworks (net5/net6/net7), empty `IByteStorage` interface.
- `Synaptix.System.Reflection` — 2 small extensions, recursive `Assembly.Load` is risky; revisit if needed.
- `Synaptix.Microsoft.Extensions` — overlap with BCL + project-specific patterns better handled in user code.
- `Synaptix.MassTransit.Kafka.Protobuf` — heavy external deps (MassTransit 8, protobuf-net 3); candidate for its own repo if revived.

## [1.0.0] — 2025-XX-XX (legacy)

Initial release of `Synaptix.Text.Case` and four other packages under
`.sln`-based monorepo. See git history for details.

[Unreleased]: https://github.com/dot-stbl/synaptix.packages/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/dot-stbl/synaptix.packages/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/dot-stbl/synaptix.packages/releases/tag/v1.0.0

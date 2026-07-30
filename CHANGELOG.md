# Changelog

All notable changes to **Synaptix Packages** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Planned
- Additional packages under `src/system/`, `src/extensions/` (if revived).
- npm packages under `src/js/` (bun workspace declared in `package.json`).
- More analyzer rules (STBL0006+): `#region` directive ban, `catch (Exception)` outside top-level handler, etc.

## [Synaptix.Analyzers 1.0.0] — 2026-XX-XX

First release of the Roslyn analyzer package. MIT licensed (community-friendly).

### Added
- `Synaptix.Analyzers` 1.0.0 — Roslyn analyzer encoding `.stbl` C# coding rules.
- 5 diagnostic rules: `STBL0001` (banned type-name suffix), `STBL0002` (banned parameter abbreviation), `STBL0003` (private method in production class), `STBL0004` (`ArgumentNullException.ThrowIfNull` in non-boundary code), `STBL0005` (`_ = await ...` discard).
- 2 CodeFixProviders shipped with v1.0: `STBL0001` (strip banned suffix), `STBL0005` (remove discard).
- `STBL0003` CodeFix deferred to v1.1 (cross-file extraction; too risky for v1.0).
- Per-package MinVer tags: `v-analyzers-<X.Y.Z>` → `Synaptix.Analyzers`; `v-text-<X.Y.Z>` → `Synaptix.Text.Case`. Global `<MinVerTagPrefix>v</MinVerTagPrefix>` removed.
- Self-host: `Synaptix.Text.Case` and `Synaptix.Text.Case.Unit` reference the analyzer via `<ProjectReference OutputItemType="Analyzer" />` so the rules fire on this repo's own code.
- Custom in-tree `AnalyzerTestRunner` (8 KB) replaces `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` 1.1.x, which hard-deps Roslyn 1.0.1 and breaks under .NET 8.
- 48 analyzer tests (xUnit + Shouldly + custom runner), covering all 5 rules + the `STBL0005` CodeFix.
- Per-rule docs under `src/analyzers/Synaptix.Analyzers/docs/rules/` (rationale, examples, CodeFix note, configuration).

### Notes
- `release.yml` rewritten to parse `v-<package>-<version>` tags, build/test only the matching project, pack exactly one `.nupkg`, and push to NuGet.org. Two tags needed for two packages.
- All 96 tests green (48 text-case + 48 analyzer). `dotnet build` 0 warnings, 0 errors.

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
- **Synaptix package icons** in `contents/` (`stbl.synaptix.png`, `stbl.synaptix.ue.png`, `stbl.synaptix.ue.2.png`, `stbl.synaptix.clr.png`, `stbl.synaptix.gzs.png`, `stbl.synaptix.gzx.png`, `stbl.synaptix.wzs.png`, `stbl.database.configuration.png`) — preserved for reuse across packages.
- **`Synaptix.Text.Case` icon**: `stbl.synaptix.ue.png` (synaptix-mark variant — synaptix as a child of `.stbl`, color-graded to fit the library's identity).
- **Brand lockup & OG image** for the monorepo itself at `assets/synaptix-packages/` ("synaptix by .stbl" — B&W, monospace, cut-out quadrant mark).

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

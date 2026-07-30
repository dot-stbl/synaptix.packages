# Changelog

All notable changes to **Synaptix Packages** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Planned
- `Synaptix.Analyzers` — separate Roslyn analyzer package (in `dot-stbl/synaptix.analyzers` repo, planned).
- Additional packages under `src/system/`, `src/extensions/` (if revived).
- npm packages under `src/js/` (bun workspace declared in `package.json`).

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

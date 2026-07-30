# Synaptix Packages

A monorepo of independent .NET libraries and Roslyn analyzers. Each package
lives in its own folder under `src/`, has independent versioning via git
tags (`v-<package>-<X.Y.Z>`), and is published to NuGet.org independently.

> **Currently shipping:** `Synaptix.Text.Case` + `Synaptix.Analyzers`. The
> monorepo structure is in place for future packages (and possibly npm
> packages under `src/js/`).

## Repository Structure

```
synaptix.packages/
├── src/
│   ├── text/
│   │   ├── Synaptix.Text.Case/              # library (GPL-3.0)
│   │   └── Synaptix.Text.Case.Unit/         # tests, side-by-side with library
│   └── analyzers/
│       ├── Synaptix.Analyzers/              # Roslyn analyzers (MIT)
│       │   └── docs/rules/<id>.md            # per-rule rationale docs
│       └── Synaptix.Analyzers.Tests/         # in-tree AnalyzerTestRunner
├── contents/                                # package icons (icon-pack)
├── assets/stbl/                             # brand submodule
├── assets/synaptix-packages/                 # monorepo lockup + OG image
├── .github/workflows/                       # ci + release + lint-commits
├── .githooks/                               # local git hooks (commit-msg)
├── scripts/                                 # format.sh/ps1, autofix.sh/ps1
├── .editorconfig                            # C# code style + analyzer severity
├── Directory.Build.props                    # strict mode + analyzer packages + MinVer
├── Directory.Packages.props                 # (CPM disabled, versions per-csproj)
├── global.json                              # pinned SDK 10.0.109
├── synaptix.packages.slnx                   # XML solution
├── package.json                             # bun workspace root (for future npm pkgs)
├── CONTRIBUTING.md                          # developer workflow
├── AGENTS.md                                # commands for AI agents
├── CHANGELOG.md                             # version history
└── LICENSE                                  # GPL-3.0-or-later
```

## Conventions

### Code style

Enforced via `.editorconfig` + analyzer packages in `Directory.Build.props`:

- **`var` everywhere** (IDE0007 = error)
- **File-scoped namespaces** (IDE0161 = error)
- **Braces always** (IDE0011 = error)
- **No expression-bodied methods** (IDE0022 = error)
- **Pattern matching** for null/type checks (IDE0078/0083 = warning, IDE0046 = error)
- **Accessibility modifiers required** (IDE0040 = error)
- **Async suffix on all `Task`/`ValueTask` returns** (VSTHRD200 = error)
- **No `.Result`/`.Wait()`** (VSTHRD103 = error)
- **No `ConfigureAwait`** in app code (RCS1090 = none — banned by global rules)

### Package versioning

- Each `.csproj` gets its version from a git tag via MinVer.
- Per-package tag prefixes:
  - `Synaptix.Text.Case` → `v-text-<X.Y.Z>` (e.g., `v-text-1.1.0`)
  - `Synaptix.Analyzers` → `v-analyzers-<X.Y.Z>` (e.g., `v-analyzers-1.0.0`)
- Pre-release tags produce `<VersionSuffix>` automatically.
- `<MinVerTagPrefix>` is set per-csproj. Global `<MinVerTagPrefix>` is **not**
  set in `Directory.Build.props` to keep versioning independent.
- `release.yml` parses the tag prefix to determine which package to build.

### License

- `GPL-3.0-or-later` (per `LICENSE` file).
- `<PackageRequireLicenseAcceptance>true</PackageRequireLicenseAcceptance>` in `Directory.Build.props`.

### Tests

- xUnit + Shouldly + coverlet. No FluentAssertions (banned globally).
- Test project lives next to its library: `src/<area>/<Lib>/` and `src/<area>/<Lib>.Unit/`.
- Test project glob in `.editorconfig`: `[**/*Unit*/**/*.cs]` (broader glob, matches anything with "Unit" in path).

### Analyzers

The `Directory.Build.props` includes:

- `Roslynator.Analyzers` 4.15.0
- `Roslynator.CodeAnalysis.Analyzers` 4.15.0
- `Microsoft.VisualStudio.Threading.Analyzers` 4.0.*

All have `<PrivateAssets>all</PrivateAssets>` so they don't leak into published packages.

`RCS1090` (ConfigureAwait) is explicitly suppressed — banned per global rules.

## Build & Test

```bash
dotnet restore synaptix.packages.slnx
dotnet build synaptix.packages.slnx -c Release       # 0 warnings, 0 errors
dotnet test synaptix.packages.slnx --no-build
```

## Adding a Package

1. Create `src/<area>/Synaptix.<Name>/Synaptix.<Name>.csproj` (use `dotnet new classlib`).
2. In csproj, set:
   - `<TargetFramework>` explicitly (don't inherit from Directory.Build.props)
   - `<PackageReadmeFile>`, `<PackageIcon>`, `<PackageTags>`, `<Description>`
   - Icon path: `<PackageIcon>stbl.synaptix.<key>.png</PackageIcon>`
   - Add `<None Include="..\..\..\contents\stbl.synaptix.<key>.png" Pack="true" PackagePath="" Link="stbl.synaptix.<key>.png" />`
3. Add to `synaptix.packages.slnx` under `<Folder Name="/src/<area>/">`.
4. (Optional) Create test project `src/<area>/Synaptix.<Name>.Unit/`.
5. (Optional) Add package icon to `contents/`.

## CI/CD

- `ci.yml` — build + test on every push/PR to master/main.
- `release.yml` — pack + push to NuGet.org on `v-*` tag (per-package).
  Parses the tag prefix to determine which project to build, packs exactly
  one `.nupkg`, verifies count, pushes, creates GitHub Release.
  Uses GitHub Environment `nuget-publish` for `NUGET_API_KEY`.
- `lint-commits.yml` — rejects AI-attribution trailers in PR commits.
- `CODEOWNERS` — `@dot-stbl/zero-team` reviews all changes.
- `dependabot.yml` — weekly NuGet + GitHub Actions updates.

## Roslyn analyzers (self-host)

`Synaptix.Analyzers` is referenced from `Synaptix.Text.Case.csproj` and
`Synaptix.Text.Case.Unit.csproj` via:

```xml
<ProjectReference Include="..\..\analyzers\Synaptix.Analyzers\Synaptix.Analyzers.csproj"
                OutputItemType="Analyzer"
                ReferenceOutputAssembly="false" />
```

This makes the analyzers load at compile time only (no transitive DLL
propagation). Switch to `<PackageReference Include="Synaptix.Analyzers" Version="1.0.0" />`
after first NuGet publish.

## Roadmap

- More analyzer rules: `STBL0006` (`#region` ban), `STBL0007`
  (`catch (Exception)` outside top-level handler), etc.
- More packages under `src/text/`, `src/system/`, etc.
- Possible npm packages under `src/js/` (workspace already declared in `package.json`).

# Contributing to Synaptix Packages

Workflow and conventions for the `.stbl` monorepo.

## Prerequisites

| Tool | Version | Why |
|------|---------|-----|
| .NET SDK | 10.0.100+ | Backend build/test |
| git | 2.30+ | Git hooks (POSIX shell) |
| bun | 1.x | Optional — bun workspace for future npm packages, script aliases |

## Initial Setup

```bash
git clone https://github.com/dot-stbl/synaptix.packages.git
cd synaptix.packages
git config core.hooksPath .githooks   # enable AI-trailer-strip hook
```

## Branch Naming

```
master                                       # main branch
feat/<short-description>                     # new feature / new package
fix/<short-description>                      # bug fix
refactor/<short-description>                 # refactor
feat/meta/<area>                             # build / tooling / docs
```

## Daily Workflow

```bash
# 1. Make your changes
# 2. Build (must pass with 0 warnings — TreatWarningsAsErrors=true)
dotnet build synaptix.packages.slnx

# 3. Test
dotnet test synaptix.packages.slnx

# 4. Format check (CI gate — same command)
bash scripts/format.sh                      # Linux/macOS
pwsh scripts/format.ps1                     # Windows

# 5. Auto-fix if format check failed
bash scripts/autofix.sh
git diff --stat                              # review before committing
```

## Commit Message Convention

Format: `[.stbl](feat/<area>): <subject>`

- `[.stbl]` — literal project tag
- `(feat/<area>)` — required feature path in parentheses; always starts with `feat/`
- `<subject>` — imperative summary, no period, ≤72 chars, lowercase

Examples:
```
[.stbl](feat/text): add ToDotCase unit tests for URLs
[.stbl](feat/meta/ci): add release workflow for NuGet push
[.stbl](feat/meta/deps): bump Roslynator to 4.15.0
[.stbl](feat/meta/rules): write analyzer rules
```

Areas: `text`, `meta`, etc. Sub-paths localize further (`feat/meta/ci`,
`feat/meta/deps`).

**Forbidden** in commit messages (stripped automatically by `.githooks/commit-msg`):
- `Co-Authored-By: Claude/Anthropic/OpenAI/...`
- `🤖 Generated with ...`
- Any AI attribution lines

## Pull Request

1. Title: short description
2. Description: what + why + verification (`dotnet build` 0/0, `dotnet test` passing, `bash scripts/format.sh` exit 0)
3. Reviewers: `@dot-stbl/zero-team`
4. CI must be green: `ci.yml` runs build + test on every PR
5. Merge: squash, delete branch

## Adding a New Package

```bash
mkdir -p src/<area>/Synaptix.<Name>
dotnet new classlib -o src/<area>/Synaptix.<Name>
# Edit csproj: add <Description>, <PackageTags>, <PackageIcon>, README inclusion
# Add to synaptix.packages.slnx under <Folder Name="/src/<area>/">
dotnet sln synaptix.packages.slnx add src/<area>/Synaptix.<Name>/Synaptix.<Name>.csproj
```

If the new package needs a test project:
```bash
mkdir -p src/<area>/Synaptix.<Name>.Unit
dotnet new xunit -o src/<area>/Synaptix.<Name>.Unit
# Edit csproj to reference library + Shouldly + test SDK
dotnet sln synaptix.packages.slnx add src/<area>/Synaptix.<Name>.Unit/Synaptix.<Name>.Unit.csproj
```

## Release Process

1. Update CHANGELOG.md with the version's changes.
2. Commit: `[.stbl](feat/meta/release): prepare v1.2.3`.
3. Tag and push:
   ```bash
   git tag v1.2.3
   git push origin v1.2.3
   ```
4. GitHub Actions `release.yml` runs automatically:
   - builds + tests
   - packs to `./artifacts/*.nupkg`
   - pushes to NuGet.org (requires `NUGET_API_KEY` in GitHub Environment `nuget-publish`)
   - creates a GitHub Release with auto-generated notes

Versioning follows [Semantic Versioning](https://semver.org/). Each package
versions independently — bumps are per-csproj, not repo-wide.

## Troubleshooting

### `git config core.hooksPath` warning
The hooks directory must exist. If you deleted `.githooks/`, restore it from
git: `git checkout HEAD -- .githooks/`.

### `dotnet build` complains about missing SDK
Check `global.json` — pinned to 10.0.109 with `rollForward: latestFeature`.
Install the matching SDK or update the pin.

### NuGet push fails with 401
The GitHub Environment `nuget-publish` must have `NUGET_API_KEY` set. Owner
configures this in GitHub → Settings → Environments → `nuget-publish`.

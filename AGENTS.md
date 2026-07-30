# AGENTS.md

Commands and conventions for AI coding agents working in this repo. Read this
before making changes.

## Quick Reference

```bash
# Build (must be 0 warnings / 0 errors)
dotnet build synaptix.packages.slnx -c Release

# Test
dotnet test synaptix.packages.slnx --no-build --nologo

# Format check
bash scripts/format.sh                       # Linux/macOS
pwsh scripts/format.ps1                      # Windows

# Auto-fix formatting
bash scripts/autofix.sh

# Pack (verify .nupkg builds)
dotnet pack synaptix.packages.slnx -c Release -o ./artifacts

# Tag a release (after CHANGELOG.md update)
git tag v1.2.3
git push origin v1.2.3   # triggers release.yml
```

## Key Conventions

- **Single NuGet package per directory under `src/`**: `src/<area>/Synaptix.<Name>/`.
- **Tests live next to their library**: `src/<area>/Synaptix.<Name>.Unit/`.
- **No FluentAssertions** — use Shouldly.
- **No `ConfigureAwait` in app code** — banned by global rules.
- **Async suffix on all async methods** (VSTHRD200 = error).
- **File-scoped namespaces, var everywhere, braces always** — enforced by `.editorconfig`.

## Critical Patterns

### Versioning

Each package versions independently via git tags + MinVer. To release:

1. Update `CHANGELOG.md` with the new version section.
2. Edit `src/<area>/Synaptix.<Name>/Synaptix.Text.Case.csproj` — bump `<PackageReleaseNotes>` if you add one, otherwise just tag.
3. Commit: `[.stbl](feat/meta/release): prepare v1.2.3`.
4. Tag: `git tag v1.2.3 && git push origin v1.2.3`.
5. CI builds + pushes to NuGet.org.

### Adding a Package

```bash
# Create structure
mkdir -p src/<area>/Synaptix.<Name>
dotnet new classlib -o src/<area>/Synaptix.<Name> --framework netstandard2.0

# Edit csproj manually — set Description, Tags, Icon path, etc.
# Add to slnx: <Project Path="src/<area>/Synaptix.<Name>/Synaptix.<Name>.csproj" />

# Test project (recommended)
dotnet new xunit -o src/<area>/Synaptix.<Name>.Unit --framework net8.0
# Edit test csproj: add ProjectReference + Shouldly + coverlet
```

## Do Not

- **Do not commit secrets** — see `~/.agents/rules/secrets.md`.
- **Do not use AutoMapper, MediatR, FluentAssertions, Moq** — banned globally; use Mapperly, hand-rolled `ICommandHandler<,>`, Shouldly, NSubstitute.
- **Do not hand-roll HTTP** — use Refit (`http-resilience-refit.md`).
- **Do not bypass the build gate** — `TreatWarningsAsErrors=true` means warnings fail the build. Fix the code, don't suppress.
- **Do not modify `Directory.Build.props` without owner approval** — engineering-zone file.

## Build Verification

Per `process/build-verification.md`, the canonical gate is:

```bash
dotnet build synaptix.packages.slnx -c Release
```

Exit 0 = pass. The build runs:
1. Compile.
2. Analyzers (CA / RCS / VSTHRD).
3. Format check (VerifyFormatOnBuild target — added in a follow-up if needed).
4. XML doc generation (CS1591 = error for public API).

## Verification Done

After making changes:
- [ ] `dotnet build synaptix.packages.slnx -c Release` — exit 0
- [ ] `dotnet test synaptix.packages.slnx --no-build` — all tests pass
- [ ] `bash scripts/format.sh` — exit 0
- [ ] No new warnings introduced (compare against `dotnet build` baseline)
- [ ] No secrets in diff (`rg -i 'token|secret|password' src/` should return nothing)

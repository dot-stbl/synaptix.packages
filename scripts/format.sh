#!/usr/bin/env bash
# scripts/format.sh — verify the solution is format-clean.
# CI gate: same command is in .github/workflows/ci.yml (via build, not this).
# Inner-loop: bash scripts/format.sh;  autofix: bash scripts/autofix.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$ROOT"
exec dotnet format synaptix.packages.slnx --verify-no-changes --severity hidden
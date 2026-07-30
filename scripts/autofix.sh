#!/usr/bin/env bash
# scripts/autofix.sh — apply safe auto-fixes across the solution.
# Same as format.sh but without --verify-no-changes. Review diff before committing.
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$ROOT"
exec dotnet format synaptix.packages.slnx --severity hidden
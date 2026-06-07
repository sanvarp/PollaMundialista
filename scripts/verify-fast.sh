#!/usr/bin/env sh
# Fast gate: format + lint only. Used by pre-commit.
set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

echo "[1/2] Frontend: prettier --check + eslint"
(cd "$ROOT/frontend" && npx prettier --check . && npm run lint)

# Backend format check only when backend files are staged (keeps commits fast).
if git diff --cached --name-only --diff-filter=ACM | grep -q '^backend/'; then
  echo "[2/2] Backend: dotnet format --verify-no-changes (backend changed)"
  (cd "$ROOT/backend" && dotnet format --verify-no-changes)
else
  echo "[2/2] Backend: skipped (no staged backend changes)"
fi

echo "OK: fast checks passed."

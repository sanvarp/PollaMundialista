#!/usr/bin/env sh
# Full quality gate: format + lint + build + test. Used by pre-push, CI, and /verify.
set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

# On Windows (git-bash) kill stray API that locks build DLLs. No-op on CI/Linux.
case "$(uname -s)" in
  MINGW* | MSYS* | CYGWIN*)
    powershell -NoProfile -Command "Get-Process -Name PollaMundialista.Api -ErrorAction SilentlyContinue | Stop-Process -Force" >/dev/null 2>&1 || true
    ;;
esac

echo "[1/5] Frontend: prettier --check"
(cd "$ROOT/frontend" && npx prettier --check .)

echo "[2/5] Frontend: eslint"
(cd "$ROOT/frontend" && npm run lint)

echo "[3/5] Backend: dotnet format --verify-no-changes"
(cd "$ROOT/backend" && dotnet format --verify-no-changes)

echo "[4/5] Backend: build + test (Release)"
(cd "$ROOT/backend" && dotnet build -c Release && dotnet test --no-build -c Release)

echo "[5/5] Frontend: ng build"
(cd "$ROOT/frontend" && npm run build)

echo "OK: all checks passed."

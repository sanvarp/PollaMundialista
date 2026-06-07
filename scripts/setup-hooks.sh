#!/usr/bin/env sh
# Enable the versioned git hooks. Run once per clone.
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit .githooks/pre-push scripts/*.sh 2>/dev/null || true
echo "Git hooks enabled (core.hooksPath=.githooks)."

# Guía de contribución — Polla Mundialista

## Setup

```bash
# Backend (.NET 10)
cd backend && dotnet restore

# Frontend (Angular 21)
cd frontend && npm install

# Activar los git hooks de calidad (una sola vez por clon)
bash scripts/setup-hooks.sh
```

## Quality gate

Todo cambio pasa por un gate automático con una **sola fuente de verdad**
(`scripts/verify-*.sh`), enforced en tres lugares:

| Momento                     | Qué corre                                                                                  |
| --------------------------- | ------------------------------------------------------------------------------------------ |
| `git commit` (pre-commit)   | `verify-fast.sh`: Prettier `--check` + ESLint; `dotnet format --verify` si tocaste `backend/`. |
| `git push` (pre-push)       | `verify-full.sh`: lo anterior + `dotnet build -c Release` + `dotnet test` + `ng build`.    |
| Pull Request / push a `main` | Los mismos checks en GitHub Actions (`ci.yml` / `deploy.yml`).                              |

Correr el gate completo a mano:

```bash
bash scripts/verify-full.sh
```

## Convenciones

- **Idioma:** UI y documentación en español; identificadores de código, mensajes de commit y comentarios en inglés.
- **Commits:** [Conventional Commits](https://www.conventionalcommits.org/) (`feat`, `fix`, `chore`, `ci`, `docs`, `build`).
- **Estilo:** Prettier + ESLint (frontend), `dotnet format` + analizadores Roslyn (backend).

## Arquitectura (resumen)

- **Backend:** ASP.NET Core (.NET 10), Clean Architecture pragmática (`Api → Application → Domain`, `Infrastructure → Application + Domain`).
- **Frontend:** Angular 21 standalone, signals, zoneless.
- **ORM/DB:** EF Core 10 — SQLite en local, Azure SQL en producción.

Ver `docs/` para los diagramas C4 y el ERD.

## Despliegue

Push a `main` → GitHub Actions corre el gate y, si pasa, despliega la API (Azure Container Apps)
y el frontend (Azure Static Web Apps) vía OIDC. Un commit que solo toca `*.md`/`docs/` no dispara deploy.

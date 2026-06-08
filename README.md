# Polla Mundialista ⚽

[![Deploy](https://github.com/sanvarp/PollaMundialista/actions/workflows/deploy.yml/badge.svg)](https://github.com/sanvarp/PollaMundialista/actions/workflows/deploy.yml)

Aplicación de predicciones para la fase de grupos del **Mundial 2026**. Los usuarios
predicen marcadores, el sistema calcula puntajes (**3 / 1 / 0**) y publica un ranking
global con historial por usuario. Pensada para un grupo privado de jugadores.

> Assessment técnico Bizagi · Rol: AI-First Full-Stack Developer.

## 🌐 App en vivo

| | URL |
|---|---|
| **Aplicación** | **https://gray-grass-037160b0f.7.azurestaticapps.net** |
| **API** | https://ca-polla-api.wittydune-a6d385e7.centralus.azurecontainerapps.io |
| **API docs (Scalar)** | _solo en entorno local_ → `/scalar/v1` |

### Credenciales demo

| Rol | Correo | Contraseña |
|---|---|---|
| **Admin** | `admin@polla.com` | `Admin#2026` |
| **User** | `user@polla.com` | `User#2026` |

Usuarios extra (rol User, contraseña `User#2026`) para ver el ranking poblado:
`ana@polla.com`, `carlos@polla.com`, `valentina@polla.com`.

> En la demo: entra como **Admin**, carga un resultado en **/admin**, y mira la **tabla
> reordenarse animada** (GSAP Flip) en **/leaderboard** (se refresca sola cada 12 s).

---

## Módulos

1. **Autenticación y usuarios** — JWT, roles `User` / `Admin`.
2. **Predicciones + lógica** — 12 partidos precargados (2 grupos), puntuación 3/1/0, bloqueo por kickoff.
3. **Panel Admin** — carga de resultados reales y recálculo de puntajes.
4. **Leaderboard + historial** — ranking global; clic en usuario → sus predicciones (con regla anti-trampa).

## Stack

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core Web API — **.NET 10 (LTS)** |
| Frontend | **Angular 21** (standalone · signals · zoneless) |
| ORM | Entity Framework Core 10 (code-first, migrations) |
| Base de datos | Azure SQL Database (Serverless) · SQLite en local |
| Auth | ASP.NET Core Identity + JWT |
| Motion FE | GSAP (SplitText · ScrollTrigger · Flip) · Lenis · Three.js (hero shader) |
| Hosting | Azure Static Web Apps (FE) + Azure Container Apps (API) |
| CI/CD | GitHub Actions (OIDC, tests como gate) |
| Observabilidad | Application Insights |

Arquitectura: **Clean Architecture pragmática (sin CQRS)**.
Diagramas y decisiones en [`/docs/architecture.md`](docs/architecture.md) · ERD en [`/docs/erd.md`](docs/erd.md).

## Estructura del repositorio

```
/backend            solución .NET (Clean Architecture)
  /src
    PollaMundialista.Api              Controllers, middleware, DI, JWT, CORS, OpenAPI/Scalar
    PollaMundialista.Application      Services, DTOs, validators, interfaces, scoring
    PollaMundialista.Domain           Entities, enums, reglas de dominio puras
    PollaMundialista.Infrastructure   EF Core, migrations, Identity, seeder
  /tests
    PollaMundialista.Tests            xUnit (35 pruebas)
  Dockerfile                          imagen multistage de la API
/frontend           app Angular 21
/infra              notas de infraestructura
/docs               arquitectura (C4), ERD, decisiones
/.github/workflows  ci.yml · deploy.yml
```

---

## Cómo levantar en local

**Requisitos:** .NET 10 SDK · Node.js LTS (20/22) · Angular CLI 21. Docker es **opcional**.

> No necesitas SQL Server: en local la API usa **SQLite** automáticamente (cero setup) y
> crea/siembra la base en el primer arranque. En producción usa Azure SQL vía migraciones.

### Backend

```bash
cd backend
dotnet run --project src/PollaMundialista.Api
# API en http://localhost:5123  ·  docs en http://localhost:5123/scalar/v1
```

### Frontend

```bash
cd frontend
npm install
npm start            # ng serve → http://localhost:4200
```

El frontend en dev apunta a `http://localhost:5123/api` (ver `src/environments/`).
CORS ya permite `http://localhost:4200`.

### Pruebas

```bash
cd backend && dotnet test      # 35 pruebas (unit + integración: scoring 3/1/0, bloqueo por kickoff, recálculo, validación, flujo E2E con WebApplicationFactory)
```

---

## Configuración (variables de entorno)

En local los valores de desarrollo viven en `appsettings.Development.json`. En producción
se inyectan como **Container App secrets / env vars** (nunca en el repo):

| Variable | Descripción |
|---|---|
| `Database__Provider` | `Sqlite` (local) o `SqlServer` (Azure) |
| `ConnectionStrings__Default` | cadena de conexión (Azure SQL en prod) |
| `Jwt__Issuer` / `Jwt__Audience` | emisor / audiencia del JWT |
| `Jwt__Secret` | clave de firma (≥ 256 bits) |
| `Jwt__ExpiryMinutes` | expiración del token (def. 60) |
| `Cors__AllowedOrigins__0` | dominio del frontend (allowlist) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | telemetría (opcional) |

---

## Despliegue (Azure + GitHub Actions)

Todo está **desplegado y automatizado**:

- **Frontend** → Azure Static Web Apps (CDN global, SSL).
- **API** → Azure Container Apps (imagen del [`Dockerfile`](backend/Dockerfile), min 1 réplica, autoscale KEDA).
- **DB** → Azure SQL Database Serverless (migraciones EF aplicadas al arrancar).
- **Observabilidad** → Application Insights.

**Pipeline** ([`.github/workflows/deploy.yml`](.github/workflows/deploy.yml)) — en cada push a `main`:

1. **`test`** (gate): `dotnet test` (35 pruebas) + `ng build`. Si falla, no se despliega.
2. **`deploy-api`**: `az acr build` (construye la imagen en la nube) → `az containerapp update`.
3. **`deploy-frontend`**: `ng build` → Azure Static Web Apps.

Autenticación a Azure por **OIDC** (federated credentials, sin secretos de larga vida).
Los PRs corren [`ci.yml`](.github/workflows/ci.yml) (build + tests) como control de calidad.

---

## Reglas de negocio (resumen)

- **Puntuación**: 3 (marcador exacto) · 1 (acierta el resultado) · 0 (resto) — [`ScoreCalculator`](backend/src/PollaMundialista.Domain/Scoring/ScoreCalculator.cs).
- **Bloqueo por kickoff**: no se puede crear/editar una predicción tras el inicio del partido.
- **Recálculo**: al cargar un resultado, se recomputan los puntos de todas las predicciones de ese partido.
- **Desempate del ranking**: `TotalPoints` ↓, luego `ExactHits` ↓, luego `DisplayName` ↑.
- **Anti-trampa**: un usuario no ve las predicciones de otro para partidos que aún no inician.

## Plan de build (milestones)

- [x] **M0** — Scaffold del monorepo
- [x] **M1** — Auth (JWT + roles)
- [x] **M2** — Dominio + seeder
- [x] **M3** — Predicciones + lógica
- [x] **M4** — Admin + recálculo
- [x] **M5** — Leaderboard + historial
- [x] **M6** — Design system + motion + hero
- [x] **M7** — Deploy (Azure + CI/CD)
- [x] **M8** — Pulido + README final

Cada milestone es un commit incremental (ver historial de git).

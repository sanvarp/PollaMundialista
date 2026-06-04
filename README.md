# Polla Mundialista ⚽

Aplicación de predicciones para la fase de grupos del **Mundial 2026**. Los usuarios
predicen marcadores, el sistema calcula puntajes (3 / 1 / 0) y publica un ranking
global con historial por usuario. Pensada para un grupo privado de jugadores.

> Assessment técnico Bizagi · Rol: AI-First Full-Stack Developer.
> _Este README se completa progresivamente; ver el plan de milestones más abajo._

---

## Módulos

1. **Autenticación y usuarios** — JWT, roles `User` / `Admin`.
2. **Predicciones + lógica de negocio** — 12 partidos precargados (2 grupos), puntuación 3/1/0, bloqueo por kickoff.
3. **Panel Admin** — carga de resultados reales y recálculo de puntajes.
4. **Leaderboard + historial** — ranking global; clic en usuario → sus predicciones (con regla anti-trampa).

## Stack

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core Web API — **.NET 10 (LTS)** |
| Frontend | **Angular 21** (standalone, signals, zoneless) |
| ORM | Entity Framework Core 10 (code-first) |
| Base de datos | Azure SQL Database (Serverless) |
| Auth | ASP.NET Core Identity + JWT |
| Motion FE | GSAP, Lenis, Three.js (hero shader) |
| Hosting | Azure Static Web Apps (FE) + Azure Container Apps (API) |
| CI/CD | GitHub Actions |
| Observabilidad | Application Insights |

Arquitectura: **Clean Architecture pragmática (sin CQRS)**. Diagramas en [`/docs`](docs/).

## Estructura del repositorio

```
/backend            solución .NET (Clean Architecture)
  /src
    PollaMundialista.Api              Controllers, middleware, DI, JWT, CORS, OpenAPI
    PollaMundialista.Application      Services, DTOs, validators, interfaces, scoring
    PollaMundialista.Domain           Entities, enums, reglas de dominio puras
    PollaMundialista.Infrastructure   EF Core, migrations, Identity, seeder
  /tests
    PollaMundialista.Tests            xUnit
/frontend           app Angular 21
/infra              Dockerfile(s), workflows, IaC
/docs               diagramas (C4, ERD), decisiones
```

## Cómo levantar local

> Documentación completa de arranque (connection string, migrations, seed,
> variables de entorno y credenciales demo) se añade en M8.

**Backend**
```bash
cd backend
dotnet build
dotnet run --project src/PollaMundialista.Api
```

**Frontend**
```bash
cd frontend
npm install
npm start            # ng serve
```

## Requisitos

- .NET 10 SDK
- Node.js LTS + Angular CLI 21
- Docker (opcional, para despliegue contenedorizado)

## Plan de build (milestones)

- [x] **M0** — Scaffold del monorepo
- [x] **M1** — Auth (JWT + roles)
- [ ] **M2** — Dominio + seeder
- [ ] **M3** — Predicciones + lógica
- [ ] **M4** — Admin + recálculo
- [ ] **M5** — Leaderboard + historial
- [ ] **M6** — Design system + motion + hero
- [ ] **M7** — Deploy (Azure + CI/CD)
- [ ] **M8** — Pulido + README final

## Credenciales demo

_Se añaden con el seeder en M2._

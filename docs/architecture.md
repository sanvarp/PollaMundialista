# Arquitectura — Polla Mundialista

## 1. Vista de despliegue (lo que está en producción)

```mermaid
flowchart LR
    user([Usuario]) -->|HTTPS| swa["Azure Static Web Apps\nAngular 21 SPA\n(CDN global)"]
    swa -->|"REST /api (JWT, CORS allowlist)"| api

    subgraph azure["Azure — rg-nexusfanatix-poc-eus2-001"]
      api["Azure Container Apps\nAPI .NET 10\nmin 1 réplica · autoscale KEDA"]
      sql[("Azure SQL Database\nServerless (autopausa)")]
      appi["Application Insights"]
      acr["Azure Container Registry"]
      api -->|"EF Core 10 (TLS)"| sql
      api -.->|telemetría| appi
      acr -.->|imagen| api
    end

    subgraph cicd["CI/CD — GitHub Actions (OIDC)"]
      gh["push a main"] --> test["test (17 pruebas) — gate"]
      test --> dapi["deploy-api: az acr build → containerapp update"]
      test --> dswa["deploy-frontend: ng build → SWA"]
    end
    dapi -.-> acr
    dswa -.-> swa
```

**Orígenes separados** (SWA y Container Apps) → la API aplica **CORS con allowlist** del dominio del SWA (nunca `*`).

## 2. Capas del backend (Clean Architecture pragmática — sin CQRS)

```mermaid
flowchart TD
    Api["PollaMundialista.Api\nControllers · JWT · CORS · OpenAPI/Scalar · rate limiting"]
    App["PollaMundialista.Application\nServices · DTOs · FluentValidation · interfaces · IApplicationDbContext"]
    Dom["PollaMundialista.Domain\nEntities · enums · ScoreCalculator (regla 3/1/0) · reglas puras"]
    Inf["PollaMundialista.Infrastructure\nEF Core 10 · Identity · migrations · seeder · TokenService"]

    Api --> App
    Api --> Inf
    App --> Dom
    Inf --> App
    Inf --> Dom
```

Regla de dependencias: siempre hacia adentro. `Infrastructure` implementa las interfaces
declaradas en `Application` (`IApplicationDbContext`, `IAuthService`, `ITokenService`).
El `Domain` no conoce EF ni Identity (las predicciones referencian al usuario por `string`).

**¿Por qué sin CQRS?** El dominio es chico (~8 operaciones). CQRS sería sobre-ingeniería;
el layering permite introducirlo si el dominio crece.

## 3. Por qué escala (4 palancas)

1. **JWT stateless** → sin sesión en servidor → escalado horizontal trivial.
2. **CDN en el edge** (SWA) → estáticos cerca del usuario, API descargada.
3. **API stateless + autoscaling KEDA** (Container Apps) → réplicas según carga.
4. **DB serverless** → autoescala vCores y autopausa sin tráfico (costo ~0 en reposo).

## 4. Camino de crecimiento (evolución, no implementado)

- **Azure Cache for Redis** para cachear el leaderboard.
- **Azure Front Door** para ruteo global / WAF.
- **Bicep** para IaC reproducible del stack completo.
- **Microsoft Entra External ID** como identidad federada (reemplazo del JWT propio).

## 5. Seguridad (resumen)

- JWT (claims `sub`/`email`/`role`, expiración configurable), hashing de Identity.
- `[Authorize(Roles="Admin")]` en endpoints admin; rate limiting en `/auth/*`.
- CORS allowlist; HTTPS en el ingress; validación server-side (FluentValidation).
- **Anti-trampa**: no se exponen predicciones ajenas de partidos no iniciados (§5.5).
- Secrets fuera del repo: variables de entorno / Container App secrets (connection
  string, JWT secret, App Insights). Evolución: Azure Key Vault.

Ver también: [ERD](erd.md).

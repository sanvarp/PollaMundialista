# infra

Infraestructura y despliegue de Polla Mundialista.

- **Dockerfile de la API**: [`../backend/Dockerfile`](../backend/Dockerfile) (multistage, .NET 10, non-root).
- **Pipelines**: [`../.github/workflows/`](../.github/workflows/) (`ci.yml`, `deploy.yml`).
- **Topología y decisiones**: [`../docs/architecture.md`](../docs/architecture.md).

## Recursos en Azure

Suscripción **GoToCloudAI**, RG `rg-nexusfanatix-poc-eus2-001`:

| Recurso | Nombre | Región |
|---|---|---|
| Static Web App (FE) | `swa-polla` | eastus2 |
| Container App (API) | `ca-polla-api` (env `cae-polla-gayn`) | centralus |
| Azure SQL Serverless | `sqlpollavawhs2` / db `pollamundialista` | centralus |
| Container Registry | `acrpollauy1a9v` | eastus2 |
| Application Insights | `appi-polla` (+ `log-polla`) | eastus2 |

> SQL y Container Apps quedaron en **centralus** porque East US/East US 2 no tenían
> capacidad para crearlos el día del despliegue. La autenticación del pipeline a Azure
> es por **OIDC** (sin secretos de larga vida).

## Evolución (IaC)

Como siguiente paso, este stack se puede capturar en **Bicep** para provisión
reproducible. Hoy fue provisionado con `az` CLI (ver `docs/architecture.md`).

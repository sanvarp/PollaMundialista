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

## IaC (Bicep)

El stack completo está capturado en [`main.bicep`](main.bicep) (parámetros de ejemplo en
[`main.bicepparam`](main.bicepparam)): ACR, Container Apps (env + app), Azure SQL
Serverless, Static Web App, Application Insights y Log Analytics.

```bash
# Validar (sin desplegar)
az bicep build --file infra/main.bicep

# Desplegar
az deployment group create -g rg-nexusfanatix-poc-eus2-001 -f infra/main.bicep \
  -p sqlAdminPassword=<pwd> jwtSecret=<secret> \
     containerImage=acrpollauy1a9v.azurecr.io/polla-api:latest \
     frontendOrigin=https://gray-grass-037160b0f.7.azurestaticapps.net
```

> El despliegue actual se hizo con `az` CLI (por restricciones de capacidad regional ese
> día); el Bicep documenta la topología de forma reproducible.

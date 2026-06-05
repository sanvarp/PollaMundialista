// ============================================================================
// Polla Mundialista — Infrastructure as Code (Azure Bicep)
//
// Captures the deployed topology: ACR + Container Apps (API) + Azure SQL
// Serverless + Static Web App + Application Insights / Log Analytics.
//
// Validate (no deploy):   az bicep build --file infra/main.bicep
// Deploy:                 az deployment group create -g <rg> -f infra/main.bicep \
//                           -p sqlAdminPassword=<pwd> jwtSecret=<secret> containerImage=<img>
// ============================================================================

@description('Region for ACR, monitoring and the Static Web App.')
param location string = 'eastus2'

@description('Region for SQL and Container Apps (centralus was used due to East US capacity).')
param computeLocation string = 'centralus'

@description('Suffix to keep globally-unique names unique.')
param suffix string = uniqueString(resourceGroup().id)

@description('Container image for the API (e.g. <acr>.azurecr.io/polla-api:<tag>).')
param containerImage string = ''

@description('Frontend origin allowed by API CORS (the Static Web App URL).')
param frontendOrigin string = ''

param sqlAdminLogin string = 'pollaadmin'

@secure()
param sqlAdminPassword string

@secure()
param jwtSecret string

// ---- Observability ---------------------------------------------------------
resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-polla'
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-polla'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logs.id
  }
}

// ---- Container Registry -----------------------------------------------------
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: 'acrpolla${suffix}'
  location: location
  sku: { name: 'Basic' }
  properties: {
    adminUserEnabled: true
  }
}

// ---- Azure SQL (Serverless) ------------------------------------------------
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'sqlpolla${suffix}'
  location: computeLocation
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlFirewall 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'pollamundialista'
  location: computeLocation
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    zoneRedundant: false
    requestedBackupStorageRedundancy: 'Local'
  }
}

// ---- Container Apps ---------------------------------------------------------
resource caEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-polla'
  location: computeLocation
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
  }
}

var sqlConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDb.name};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;'

resource api 'Microsoft.App/containerApps@2024-03-01' = if (!empty(containerImage)) {
  name: 'ca-polla-api'
  location: computeLocation
  properties: {
    managedEnvironmentId: caEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      registries: [
        {
          server: acr.properties.loginServer
          username: acr.listCredentials().username
          passwordSecretRef: 'acr-pwd'
        }
      ]
      secrets: [
        { name: 'acr-pwd', value: acr.listCredentials().passwords[0].value }
        { name: 'connstr', value: sqlConnectionString }
        { name: 'jwtsecret', value: jwtSecret }
        { name: 'appinsights', value: appInsights.properties.ConnectionString }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: containerImage
          resources: { cpu: json('0.5'), memory: '1.0Gi' }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'Database__Provider', value: 'SqlServer' }
            { name: 'ConnectionStrings__Default', secretRef: 'connstr' }
            { name: 'Jwt__Issuer', value: 'PollaMundialista' }
            { name: 'Jwt__Audience', value: 'PollaMundialista.Client' }
            { name: 'Jwt__Secret', secretRef: 'jwtsecret' }
            { name: 'Jwt__ExpiryMinutes', value: '60' }
            { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', secretRef: 'appinsights' }
            { name: 'Cors__AllowedOrigins__0', value: frontendOrigin }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
        rules: [
          {
            name: 'http-scale'
            http: { metadata: { concurrentRequests: '50' } }
          }
        ]
      }
    }
  }
}

// ---- Static Web App ---------------------------------------------------------
resource swa 'Microsoft.Web/staticSites@2023-12-01' = {
  name: 'swa-polla'
  location: location
  sku: { name: 'Free', tier: 'Free' }
  properties: {}
}

// ---- Outputs ---------------------------------------------------------------
output acrLoginServer string = acr.properties.loginServer
output apiFqdn string = api.?properties.configuration.ingress.fqdn ?? ''
output swaHostname string = swa.properties.defaultHostname
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName

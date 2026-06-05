using './main.bicep'

// Example parameters. Secrets (sqlAdminPassword, jwtSecret) should be passed at
// deploy time or sourced from Key Vault — never committed.
param location = 'eastus2'
param computeLocation = 'centralus'
param containerImage = 'acrpolla0000.azurecr.io/polla-api:latest'
param frontendOrigin = 'https://gray-grass-037160b0f.7.azurestaticapps.net'
param sqlAdminPassword = ''
param jwtSecret = ''

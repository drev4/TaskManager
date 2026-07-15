// TaskManager - Ultra Free Deployment (SQLite + Free App Service)
param location string = resourceGroup().location

@description('Enables JWT auth enforcement (Auth:Enabled). Leave false until a real Microsoft Entra External ID tenant is configured.')
param authEnabled bool = false
param azureAdInstance string = ''
param azureAdTenantId string = ''
param azureAdClientId string = ''
@description('Production frontend origin, appended to AllowedOrigins for CORS.')
param productionFrontendUrl string = ''

// Variables
var appName = 'taskmgr${uniqueString(resourceGroup().id)}'
var storageName = 'taskmgrstorage${uniqueString(resourceGroup().id)}'

// App Service Plan - FREE TIER
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${appName}-plan'
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
    family: 'F'
    capacity: 0
  }
  properties: {
    reserved: false
  }
}

// App Service - FREE TIER
resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      scmType: 'None'
      use32BitWorkerProcess: false
      alwaysOn: false // Not available in Free tier
      webSocketsEnabled: true
      requestTracingEnabled: true
      httpLoggingEnabled: true
      logsDirectorySizeLimit: 40
      detailedErrorLoggingEnabled: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'DATABASE_TYPE'
          value: 'SQLite'
        }
        {
          name: 'Auth__Enabled'
          value: string(authEnabled)
        }
        {
          name: 'AzureAd__Instance'
          value: azureAdInstance
        }
        {
          name: 'AzureAd__TenantId'
          value: azureAdTenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: azureAdClientId
        }
        {
          // appsettings.json's AllowedOrigins array already fills indices 0-5 with
          // localhost dev origins; the env-var config provider merges array entries
          // by index, so this adds the production origin without dropping those.
          name: 'AllowedOrigins__6'
          value: productionFrontendUrl
        }
      ]
    }
  }
}

// Storage Account - Hot tier (minimal cost)
resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

// Outputs
output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output storageAccountName string = storage.name

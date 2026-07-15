param location string = resourceGroup().location
param sqlAdmin string
@secure()
param sqlPassword string
param useFreeResources bool = true

@description('Enables JWT auth enforcement (Auth:Enabled). Leave false until a real Microsoft Entra External ID tenant is configured.')
param authEnabled bool = false
param azureAdInstance string = ''
param azureAdTenantId string = ''
param azureAdClientId string = ''
@description('Production frontend origin, appended to AllowedOrigins for CORS.')
param productionFrontendUrl string = ''

// Variables
var appName = 'taskmgr${uniqueString(resourceGroup().id)}'
var sqlServerName = 'taskmgrsql${uniqueString(resourceGroup().id)}'
var storageName = 'taskmgrstorage${uniqueString(resourceGroup().id)}'

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdmin
    administratorLoginPassword: sqlPassword
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Database
resource database 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: 'taskmgr'
  location: location
  sku: useFreeResources ? {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  } : {
    name: 'S0'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: useFreeResources ? 2147483648 : 268435456000 // 2GB for free, 250GB for paid
  }
}

// Firewall rule to allow Azure services
resource firewallRule 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${appName}-plan'
  location: location
  sku: useFreeResources ? {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
    family: 'F'
    capacity: 0
  } : {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  properties: {
    reserved: false
  }
}

// App Service
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
      alwaysOn: useFreeResources ? false : true // AlwaysOn not available in Free tier
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
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${database.name};Persist Security Info=False;User ID=${sqlAdmin};Password=${sqlPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
          type: 'SQLAzure'
        }
      ]
    }
  }
}

// Storage Account
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
output sqlServerName string = sqlServer.name
output databaseName string = database.name
output storageAccountName string = storage.name
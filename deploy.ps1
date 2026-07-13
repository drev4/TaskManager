# TaskManager Azure Deployment Script
param(
    [string]$subscriptionId,
    [string]$resourceGroupName = "rg-taskmgr-portfolio",
    [string]$location = "East US",
    [Parameter(Mandatory = $true)]
    [string]$sqlAdminPassword
)

Write-Host "🚀 Starting TaskManager deployment to Azure..." -ForegroundColor Green

# Check if Azure CLI is installed
if (!(Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure CLI not found. Please install Azure CLI first." -ForegroundColor Red
    Write-Host "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows" -ForegroundColor Yellow
    exit 1
}

# Login to Azure (if not already logged in)
Write-Host "🔐 Checking Azure authentication..." -ForegroundColor Yellow
$loginCheck = az account show --output none 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please login to Azure..." -ForegroundColor Yellow
    az login
}

# Set subscription if provided
if ($subscriptionId) {
    Write-Host "📋 Setting subscription: $subscriptionId" -ForegroundColor Yellow
    az account set --subscription $subscriptionId
}

# Get current subscription
$currentSub = az account show --query "name" -o tsv
Write-Host "✅ Using subscription: $currentSub" -ForegroundColor Green

# Create resource group
Write-Host "📁 Creating resource group: $resourceGroupName" -ForegroundColor Yellow
az group create --name $resourceGroupName --location $location

# Deploy infrastructure
Write-Host "🏗️ Deploying infrastructure with Bicep..." -ForegroundColor Yellow
$deployResult = az deployment group create `
    --resource-group $resourceGroupName `
    --template-file "infra/main.bicep" `
    --parameters "@infra/main.parameters.json" `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Infrastructure deployed successfully!" -ForegroundColor Green
    
    # Extract outputs
    $appServiceName = $deployResult.properties.outputs.appServiceName.value
    $appServiceUrl = $deployResult.properties.outputs.appServiceUrl.value
    $sqlServerName = $deployResult.properties.outputs.sqlServerName.value
    $databaseName = $deployResult.properties.outputs.databaseName.value
    
    Write-Host "📊 Deployment Details:" -ForegroundColor Cyan
    Write-Host "  - App Service: $appServiceName" -ForegroundColor White
    Write-Host "  - URL: $appServiceUrl" -ForegroundColor White
    Write-Host "  - SQL Server: $sqlServerName" -ForegroundColor White
    Write-Host "  - Database: $databaseName" -ForegroundColor White
    
    # Build and deploy the API
    Write-Host "🔨 Building and deploying API..." -ForegroundColor Yellow
    
    # Navigate to API directory
    Set-Location "api/TaskMgr.Api"
    
    # Publish the app
    dotnet publish -c Release -o "./publish"
    
    # Create deployment package
    Compress-Archive -Path "./publish/*" -DestinationPath "./deploy.zip" -Force
    
    # Deploy to App Service
    az webapp deployment source config-zip `
        --resource-group $resourceGroupName `
        --name $appServiceName `
        --src "./deploy.zip"
    
    # Run database migrations
    Write-Host "🗄️ Running database migrations..." -ForegroundColor Yellow
    $connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=$databaseName;Persist Security Info=False;User ID=taskmgradmin;Password=$sqlAdminPassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    # Update connection string temporarily for migrations
    $originalConnectionString = (Get-Content "../TaskMgr.Api/appsettings.json" | ConvertFrom-Json).ConnectionStrings.DefaultConnection
    $appSettings = Get-Content "../TaskMgr.Api/appsettings.json" | ConvertFrom-Json
    $appSettings.ConnectionStrings.DefaultConnection = $connectionString
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content "../TaskMgr.Api/appsettings.json"
    
    # Run migrations
    dotnet ef database update
    
    # Restore original connection string
    $appSettings.ConnectionStrings.DefaultConnection = $originalConnectionString
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content "../TaskMgr.Api/appsettings.json"
    
    # Clean up
    Remove-Item "./publish" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./deploy.zip" -Force -ErrorAction SilentlyContinue
    
    # Return to root
    Set-Location "../.."
    
    Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
    Write-Host "🌐 Your API is available at: $appServiceUrl" -ForegroundColor Cyan
    Write-Host "📖 Swagger documentation: $appServiceUrl/swagger" -ForegroundColor Cyan
    Write-Host "❤️ Health check: $appServiceUrl/health" -ForegroundColor Cyan
    
} else {
    Write-Host "❌ Infrastructure deployment failed!" -ForegroundColor Red
    exit 1
}

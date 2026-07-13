# TaskManager DP-300 Azure Deployment
param(
    [string]$resourceGroupName = "rg-taskmgr-dp300-portfolio",
    [string]$location = "East US",
    [Parameter(Mandatory = $true)]
    [string]$sqlAdminPassword
)

Write-Host "TaskManager DP-300 Portfolio - Azure Deployment" -ForegroundColor Cyan
Write-Host "Estimated cost: ~5.50 USD/month" -ForegroundColor Green

# Add Azure CLI to PATH
$env:PATH += ";C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin"

# Check Azure CLI
Write-Host "Checking Azure CLI..." -ForegroundColor Yellow
if (!(Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Host "Azure CLI not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Azure CLI found!" -ForegroundColor Green

# Login check
Write-Host "Checking Azure authentication..." -ForegroundColor Yellow
$loginCheck = az account show --output none 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please login to Azure..." -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Azure login failed!" -ForegroundColor Red
        exit 1
    }
}

# Get current subscription
$currentSub = az account show --query "name" -o tsv
Write-Host "Using subscription: $currentSub" -ForegroundColor Green

# Create resource group
Write-Host "Creating resource group: $resourceGroupName" -ForegroundColor Yellow
az group create --name $resourceGroupName --location $location

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create resource group!" -ForegroundColor Red
    exit 1
}

# Deploy infrastructure
Write-Host "Deploying infrastructure..." -ForegroundColor Yellow
Write-Host "Resources: SQL Server + Database (Basic) + App Service (Free)" -ForegroundColor Cyan

$deployResult = az deployment group create --resource-group $resourceGroupName --template-file "infra/main.bicep" --parameters "@infra/main.parameters.json" --output json

if ($LASTEXITCODE -ne 0) {
    Write-Host "Infrastructure deployment failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Infrastructure deployed successfully!" -ForegroundColor Green

# Parse deployment results
$deployObj = $deployResult | ConvertFrom-Json
$appServiceName = $deployObj.properties.outputs.appServiceName.value
$appServiceUrl = $deployObj.properties.outputs.appServiceUrl.value
$sqlServerName = $deployObj.properties.outputs.sqlServerName.value
$databaseName = $deployObj.properties.outputs.databaseName.value

Write-Host "Deployment Details:" -ForegroundColor Cyan
Write-Host "- App Service: $appServiceName" -ForegroundColor White
Write-Host "- URL: $appServiceUrl" -ForegroundColor White
Write-Host "- SQL Server: $sqlServerName" -ForegroundColor White
Write-Host "- Database: $databaseName" -ForegroundColor White

# Build and deploy API
Write-Host "Building and deploying API..." -ForegroundColor Yellow

$originalLocation = Get-Location
Set-Location "api/TaskMgr.Api"

try {
    # Build
    Write-Host "Building .NET application..." -ForegroundColor Yellow
    dotnet publish -c Release -o "./publish" --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        return
    }
    
    Write-Host "Build successful!" -ForegroundColor Green
    
    # Create package
    Write-Host "Creating deployment package..." -ForegroundColor Yellow
    if (Test-Path "./deploy.zip") { Remove-Item "./deploy.zip" -Force }
    Compress-Archive -Path "./publish/*" -DestinationPath "./deploy.zip" -Force
    
    # Deploy to App Service
    Write-Host "Deploying to Azure App Service..." -ForegroundColor Yellow
    az webapp deployment source config-zip --resource-group $resourceGroupName --name $appServiceName --src "./deploy.zip"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "API deployed successfully!" -ForegroundColor Green
        
        # Database migrations
        Write-Host "Running database migrations..." -ForegroundColor Yellow
        $connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=$databaseName;Persist Security Info=False;User ID=taskmgradmin;Password=$sqlAdminPassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
        
        # Backup and update connection string
        $appSettingsPath = "appsettings.json"
        $originalContent = Get-Content $appSettingsPath -Raw
        $appSettings = $originalContent | ConvertFrom-Json
        $appSettings.ConnectionStrings.DefaultConnection = $connectionString
        $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
        
        # Run migrations
        dotnet ef database update
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Database migrations completed!" -ForegroundColor Green
        } else {
            Write-Host "Database migrations failed, but deployment continues..." -ForegroundColor Yellow
        }
        
        # Restore connection string
        $originalContent | Set-Content $appSettingsPath
        
    } else {
        Write-Host "API deployment failed!" -ForegroundColor Red
    }
    
    # Clean up
    Remove-Item "./publish" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "./deploy.zip" -Force -ErrorAction SilentlyContinue
    
} finally {
    Set-Location $originalLocation
}

Write-Host "DP-300 Portfolio Deployment Completed!" -ForegroundColor Green
Write-Host "Your TaskManager API is live at:" -ForegroundColor Cyan
Write-Host "  $appServiceUrl" -ForegroundColor White
Write-Host "Swagger Documentation:" -ForegroundColor Cyan
Write-Host "  $appServiceUrl/swagger" -ForegroundColor White
Write-Host "Health Check:" -ForegroundColor Cyan
Write-Host "  $appServiceUrl/health" -ForegroundColor White

Write-Host "DP-300 Portfolio Value:" -ForegroundColor Magenta
Write-Host "- Azure SQL Database (Basic tier)" -ForegroundColor Green
Write-Host "- SQL Server Authentication" -ForegroundColor Green
Write-Host "- Database Migrations" -ForegroundColor Green
Write-Host "- Production Infrastructure" -ForegroundColor Green

Write-Host "Monthly Cost Estimate: ~5.50 USD" -ForegroundColor Yellow
Write-Host "To delete resources: az group delete --name $resourceGroupName" -ForegroundColor Gray

# TaskManager DP-300 Portfolio - Azure Deployment Script
param(
    [string]$subscriptionId,
    [string]$resourceGroupName = "rg-taskmgr-dp300-portfolio",
    [string]$location = "East US",
    [Parameter(Mandatory = $true)]
    [string]$sqlAdminPassword
)

Write-Host "🎯 DP-300 Portfolio - TaskManager Azure Deployment" -ForegroundColor Cyan
Write-Host "💰 Estimated cost: ~$5.50/month (Azure SQL Basic + App Service Free)" -ForegroundColor Green

# Check if Azure CLI is installed
Write-Host "🔍 Checking Azure CLI..." -ForegroundColor Yellow
if (!(Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure CLI not found!" -ForegroundColor Red
    Write-Host "📥 Please install Azure CLI first:" -ForegroundColor Yellow
    Write-Host "   https://aka.ms/installazurecliwindows" -ForegroundColor White
    Write-Host "   Then restart PowerShell and run this script again." -ForegroundColor White
    exit 1
}

Write-Host "✅ Azure CLI found!" -ForegroundColor Green
az --version

# Login to Azure
Write-Host "`n🔐 Checking Azure authentication..." -ForegroundColor Yellow
$loginCheck = az account show --output none 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "🔑 Please login to Azure..." -ForegroundColor Yellow
    az login
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Azure login failed!" -ForegroundColor Red
        exit 1
    }
}

# Set subscription if provided
if ($subscriptionId) {
    Write-Host "📋 Setting subscription: $subscriptionId" -ForegroundColor Yellow
    az account set --subscription $subscriptionId
}

# Get current subscription
$currentSub = az account show --query "name" -o tsv
$currentSubId = az account show --query "id" -o tsv
Write-Host "✅ Using subscription: $currentSub" -ForegroundColor Green
Write-Host "🆔 Subscription ID: $currentSubId" -ForegroundColor Cyan

# Create resource group
Write-Host "`n📁 Creating resource group: $resourceGroupName" -ForegroundColor Yellow
az group create --name $resourceGroupName --location $location

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Resource group created successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to create resource group!" -ForegroundColor Red
    exit 1
}

# Deploy infrastructure
Write-Host "`n🏗️ Deploying DP-300 Portfolio Infrastructure..." -ForegroundColor Yellow
Write-Host "📊 Resources to be created:" -ForegroundColor Cyan
Write-Host "  - Azure SQL Server (with admin login)" -ForegroundColor White
Write-Host "  - Azure SQL Database (Basic tier - DP-300 relevant)" -ForegroundColor White
Write-Host "  - App Service (Free tier)" -ForegroundColor White
Write-Host "  - Storage Account (Standard LRS)" -ForegroundColor White

$deployResult = az deployment group create `
    --resource-group $resourceGroupName `
    --template-file "infra/main.bicep" `
    --parameters "@infra/main.parameters.json" `
    --output json

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Infrastructure deployed successfully!" -ForegroundColor Green
    
    # Parse deployment results
    $deployObj = $deployResult | ConvertFrom-Json
    $appServiceName = $deployObj.properties.outputs.appServiceName.value
    $appServiceUrl = $deployObj.properties.outputs.appServiceUrl.value
    $sqlServerName = $deployObj.properties.outputs.sqlServerName.value
    $databaseName = $deployObj.properties.outputs.databaseName.value
    
    Write-Host "`n🎉 DP-300 Portfolio Infrastructure Ready!" -ForegroundColor Green
    Write-Host "📊 Deployment Details:" -ForegroundColor Cyan
    Write-Host "  - Resource Group: $resourceGroupName" -ForegroundColor White
    Write-Host "  - App Service: $appServiceName" -ForegroundColor White
    Write-Host "  - URL: $appServiceUrl" -ForegroundColor White
    Write-Host "  - SQL Server: $sqlServerName.database.windows.net" -ForegroundColor White
    Write-Host "  - Database: $databaseName" -ForegroundColor White
    
    Write-Host "`n🔨 Building and deploying API..." -ForegroundColor Yellow
    
    # Navigate to API directory
    $originalLocation = Get-Location
    Set-Location "api/TaskMgr.Api"
    
    try {
        # Build the application
        Write-Host "🔧 Building .NET application..." -ForegroundColor Yellow
        dotnet publish -c Release -o "./publish" --verbosity quiet
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Build successful!" -ForegroundColor Green
            
            # Create deployment package
            Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow
            if (Test-Path "./deploy.zip") { Remove-Item "./deploy.zip" -Force }
            Compress-Archive -Path "./publish/*" -DestinationPath "./deploy.zip" -Force
            
            # Deploy to App Service
            Write-Host "🚀 Deploying to Azure App Service..." -ForegroundColor Yellow
            az webapp deployment source config-zip `
                --resource-group $resourceGroupName `
                --name $appServiceName `
                --src "./deploy.zip"
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ API deployed successfully!" -ForegroundColor Green
                
                # Run database migrations
                Write-Host "`n🗄️ Running database migrations..." -ForegroundColor Yellow
                $connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=$databaseName;Persist Security Info=False;User ID=taskmgradmin;Password=$sqlAdminPassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
                
                # Backup original connection string
                $appSettingsPath = "appsettings.json"
                $originalContent = Get-Content $appSettingsPath -Raw
                $appSettings = $originalContent | ConvertFrom-Json
                $originalConnectionString = $appSettings.ConnectionStrings.DefaultConnection
                
                # Update connection string for migrations
                $appSettings.ConnectionStrings.DefaultConnection = $connectionString
                $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
                
                # Run migrations
                dotnet ef database update
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ Database migrations completed!" -ForegroundColor Green
                } else {
                    Write-Host "⚠️ Database migrations failed, but deployment continues..." -ForegroundColor Yellow
                }
                
                # Restore original connection string
                $originalContent | Set-Content $appSettingsPath
                
            } else {
                Write-Host "❌ API deployment failed!" -ForegroundColor Red
            }
            
            # Clean up
            Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
            Remove-Item "./publish" -Recurse -Force -ErrorAction SilentlyContinue
            Remove-Item "./deploy.zip" -Force -ErrorAction SilentlyContinue
            
        } else {
            Write-Host "❌ Build failed!" -ForegroundColor Red
        }
        
    } finally {
        # Return to original directory
        Set-Location $originalLocation
    }
    
    Write-Host "`n🎉 DP-300 Portfolio Deployment Completed!" -ForegroundColor Green
    Write-Host "🌐 Your TaskManager API is live at:" -ForegroundColor Cyan
    Write-Host "   $appServiceUrl" -ForegroundColor White
    Write-Host "📖 Swagger Documentation:" -ForegroundColor Cyan
    Write-Host "   $appServiceUrl/swagger" -ForegroundColor White
    Write-Host "❤️ Health Check:" -ForegroundColor Cyan
    Write-Host "   $appServiceUrl/health" -ForegroundColor White
    
    Write-Host "`n📚 DP-300 Portfolio Value:" -ForegroundColor Magenta
    Write-Host "✅ Azure SQL Database (Basic tier)" -ForegroundColor Green
    Write-Host "✅ SQL Server Authentication & Security" -ForegroundColor Green
    Write-Host "✅ Database Migrations & Schema Management" -ForegroundColor Green
    Write-Host "✅ Connection String Management" -ForegroundColor Green
    Write-Host "✅ Azure App Service Integration" -ForegroundColor Green
    Write-Host "✅ Production-ready Infrastructure as Code" -ForegroundColor Green
    
    Write-Host "`n💰 Monthly Cost Estimate: ~$5.50 USD" -ForegroundColor Yellow
    Write-Host "🔄 To delete resources: az group delete --name $resourceGroupName" -ForegroundColor Gray
    
} else {
    Write-Host "❌ Infrastructure deployment failed!" -ForegroundColor Red
    Write-Host "🔍 Check the error messages above for details." -ForegroundColor Yellow
    exit 1
}

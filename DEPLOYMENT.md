# Azure Deployment

Deploys the API to Azure using the Bicep templates in `infra/`.

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An Azure subscription with Contributor/Owner access

## What gets created

- Resource group
- Azure SQL Server + Database (S0 tier)
- App Service Plan (Basic B1) + App Service
- Storage Account (reserved for future file attachments)

Rough cost at this tier: ~$30-35/month (SQL ~$15, App Service ~$13, Storage ~$1).

## Deploy

### Scripted

```powershell
.\deploy.ps1
```
Prompts for Azure login, then provisions infrastructure and deploys the API (~5-10 minutes).

### Manual

```bash
az login

az group create --name rg-taskmgr-portfolio --location "East US"

az deployment group create \
  --resource-group rg-taskmgr-portfolio \
  --template-file infra/main.bicep \
  --parameters @infra/main.parameters.json

cd api/TaskMgr.Api
dotnet publish -c Release -o ./publish
zip -r deploy.zip ./publish/*

APP_NAME=$(az webapp list --resource-group rg-taskmgr-portfolio --query "[0].name" -o tsv)
az webapp deployment source config-zip \
  --resource-group rg-taskmgr-portfolio \
  --name $APP_NAME \
  --src deploy.zip

# point the connection string at the new Azure SQL instance, then:
dotnet ef database update
```

## Configuration

`infra/main.parameters.json` is gitignored since it holds the SQL admin credentials. Copy the example and fill in real values before deploying:

```bash
cp infra/main.parameters.example.json infra/main.parameters.json
```

App Service gets `ASPNETCORE_ENVIRONMENT=Production` and `ConnectionStrings__DefaultConnection` set automatically from the deployment.

## Verifying a deployment

```
GET https://<app-name>.azurewebsites.net/health
GET https://<app-name>.azurewebsites.net/swagger
```

## Troubleshooting

- **SQL connection timeout** — check the server firewall rules; "Allow Azure services" needs to be enabled.
- **Deployment fails** — check App Service logs in the portal, and confirm the .NET 8 runtime stack is selected.
- **Permission errors** — confirm your account has the Contributor role: `az role assignment list --assignee <email>`.

## Not set up yet

- CI/CD pipeline (GitHub Actions / Azure DevOps)
- Custom domain / SSL
- Application Insights
- Frontend deployment (Azure Static Web Apps)

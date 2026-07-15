# TaskMgr.Api

.NET 8 Web API for the TaskManager app: Clean Architecture, EF Core, and optional Microsoft Entra External ID auth. See the [root README](../../README.md) for the overall architecture and technology rationale — this file covers API-specific setup.

## Setup

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
dotnet run
```

- HTTP: `http://localhost:5000` (LocalDB dev profile: `http://localhost:65454`)
- Swagger: `/swagger`

### Microsoft Entra External ID (optional)

Enforcement is controlled by the `Auth:Enabled` flag (`appsettings.json`, or `Auth__Enabled` app setting when deployed) — `false` by default, so REST controllers and the `TaskHub` SignalR hub fall back to a mock user (see root README) without needing a tenant. To enable it, set `Auth:Enabled` to `true` and fill in `AzureAd` with your external tenant's details:

```json
{
  "Auth": {
    "Enabled": true
  },
  "AzureAd": {
    "Instance": "https://<tenant-subdomain>.ciamlogin.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<api-client-id>"
  }
}
```

Prefer `dotnet user-secrets` over committing real tenant values to `appsettings.json`.

### Local database

`appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskMgrDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  }
}
```

## Project layout

```
TaskMgr.Api/
├── Domain/           # Entities, domain events, enums, repository interfaces
├── Application/      # Commands/queries (MediatR), DTOs, validators, AutoMapper profiles, event handlers
├── Infrastructure/   # Repository implementations, SignalR hubs
├── Controllers/       # Thin HTTP layer
├── Middleware/
├── Extensions/        # DI setup, split by concern (AddDatabase, AddAuthenticationServices, ...)
└── Data/              # DbContext, EF Core migrations
```

## Endpoints

| Area | Routes |
|---|---|
| Users | `GET/PUT /api/users/me`, `POST /api/users/initialize` |
| Projects | `GET/POST /api/projects`, `GET/PUT/DELETE /api/projects/{id}` |
| Tasks | `GET/POST /api/tasks`, `GET/PUT/DELETE /api/tasks/{id}`, `POST /api/tasks/{id}/time` |
| Dashboard | `GET /api/dashboard/stats`, `/activity`, `/trends` |

Full request/response schemas are in Swagger at `/swagger`.

## Health checks

`/health`, `/health/ready`, `/health/live` — covers the API process and the SQL Server connection.

## Logging

Serilog: console sink in development, rolling file sink in production, with request correlation IDs.

## Testing

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

## Docker

```bash
docker build -t taskmanager-api .
docker run -p 5000:80 taskmanager-api
```

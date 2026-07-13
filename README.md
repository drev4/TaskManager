# TaskManager

A full-stack task and project management application built with .NET 8 and Vue 3. It covers project/task CRUD, time tracking, dashboard analytics, and live updates over SignalR.

This is a personal project used to practice and demonstrate a Clean Architecture / CQRS backend paired with a typed Vue 3 frontend, rather than a from-scratch business product.

## Tech stack and why

**.NET 8 / ASP.NET Core** — statically typed, mature ecosystem, and the framework I have the most production experience with. Minimal API overhead isn't a priority here; a controller-based Web API keeps the project structure familiar to anyone coming from an enterprise .NET background.

**Clean Architecture (Domain / Application / Infrastructure / API)** — keeps EF Core and SignalR out of the domain layer so business rules (e.g. task status transitions) can be unit tested without spinning up a database.

**MediatR (CQRS) + domain events** — commands like `CreateTask` go through a handler pipeline instead of being inlined in controllers, and side effects (e.g. notifying a SignalR hub when a task is created) live in a separate `TaskCreatedEventHandler` rather than being bolted onto the command itself. This keeps the "create a task" use case focused on one thing and makes it easy to add more reactions to the same event later.

**Entity Framework Core + SQL Server** — code-first migrations were the fastest way to iterate on the schema, and SQL Server LocalDB removes any setup friction for local development. A repository/unit-of-work layer sits on top so the application layer doesn't depend on `DbContext` directly.

**SignalR** — the dashboard needs to reflect task changes without polling; a hub pushes updates to connected clients when tasks are created or change status.

**FluentValidation over Data Annotations** — validation rules for DTOs (e.g. conditional rules, cross-field checks) get complex quickly, and FluentValidation keeps them out of the model classes and independently testable.

**Serilog + OpenTelemetry + health checks** — structured logging and traces are what actually get used when something breaks in a deployed environment; console logging alone isn't enough to reason about latency or correlate requests.

**Redis (output/response caching) + Hangfire** — added for read-heavy dashboard queries and for background/recurring jobs (e.g. periodic aggregation), so the request pipeline isn't doing that work synchronously.

**Vue 3 + TypeScript + Composition API** — smaller mental model than React for this size of app, and the Composition API keeps related state and logic (e.g. the `useSignalR` composable) together instead of scattered across lifecycle hooks.

**Pinia** — the currently recommended store for Vue 3; simpler API and better TypeScript inference than Vuex.

**Tailwind CSS** — avoids maintaining a separate stylesheet per component for a UI this size; utility classes are also easier to keep consistent across a small team or solo project.

**Azure AD B2C (optional)** — the API is wired for it via `Microsoft.Identity.Web`, but it's disabled by default in local development in favor of a mock user, so the app can be cloned and run without an Azure tenant.

## Architecture

```
Domain            Entities, domain events, enums, repository interfaces
Application       Commands/queries (MediatR), DTOs, validators, AutoMapper profiles, event handlers
Infrastructure     EF Core repositories, SignalR hubs, external services
Controllers        Thin HTTP layer that only translates requests into MediatR commands/queries
```

Dependencies point inward: Domain has no dependency on Infrastructure or EF Core.

## Project layout

```
TaskManager/
├── api/TaskMgr.Api/
│   ├── Application/       # Commands, DTOs, validators, mappings, event handlers
│   ├── Controllers/       # API controllers
│   ├── Data/               # DbContext and EF Core migrations
│   ├── Domain/             # Entities, domain events, enums, repository interfaces
│   └── Infrastructure/     # Repository implementations, SignalR hubs
│
├── frontend/
│   └── src/
│       ├── components/     # Reusable Vue components
│       ├── composables/    # e.g. useSignalR
│       ├── pages/          # Route-level views
│       ├── router/
│       ├── stores/         # Pinia stores
│       └── types/
│
└── infra/                  # Azure infrastructure (Bicep)
```

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- Git

### Backend

```bash
cd api/TaskMgr.Api
dotnet restore
dotnet ef database update

# optional: seed sample data
sqlcmd -S "(localdb)\mssqllocaldb" -d TaskMgrDb -i Data/seed-data.sql

dotnet run
```

- API: http://localhost:65454 (https: 65453)
- Swagger: http://localhost:65454/swagger

### Frontend

```bash
cd frontend
npm install
cp .env.example .env   # set VITE_API_BASE_URL if it differs from the default
npm run dev
```

Runs at http://localhost:3001.

### Authentication in local development

Azure AD B2C is disabled by default; the API uses a mock user (seeded as "Bob Wilson") so the app runs without an Azure tenant. To enable real auth: configure a B2C tenant, set the corresponding values in `.env`, uncomment the `[Authorize]` attributes in the controllers, and remove the mock-user fallback in `GetCurrentUserId()`.

## Testing

Backend:
```bash
cd api/TaskMgr.Api
dotnet test
```

Frontend:
```bash
cd frontend
npm run test        # vitest
npm run type-check   # vue-tsc
npm run lint
```

## Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for deploying to Azure with the Bicep templates in `infra/`.

## License

MIT

# TaskManager

A modern, full-stack task management platform built with .NET 8 and Vue 3. This application helps teams organize projects, track tasks, and collaborate effectively with a clean, intuitive interface.

## Features

- **Project Management**: Create and organize projects with color coding and due dates
- **Task Tracking**: Comprehensive task management with status, priority, assignments, and time tracking
- **Dashboard Analytics**: Real-time statistics and insights into project progress
- **User Management**: Multi-user support with role-based access
- **Time Tracking**: Estimate and track actual hours spent on tasks
- **Filtering & Search**: Advanced filtering and search capabilities
- **Responsive Design**: Modern, mobile-friendly UI built with Tailwind CSS

## Tech Stack

### Backend
- **.NET 8**: Modern, high-performance web API
- **Entity Framework Core 8**: Database access and migrations
- **SQL Server**: Relational database (with LocalDB support for development)
- **Clean Architecture**: Domain-Driven Design (DDD) with CQRS pattern
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation

### Frontend
- **Vue 3**: Progressive JavaScript framework with Composition API
- **TypeScript**: Type-safe development
- **Vite**: Fast build tool and dev server
- **Pinia**: State management
- **Vue Router 4**: Client-side routing
- **Tailwind CSS**: Utility-first CSS framework
- **Axios**: HTTP client
- **Heroicons**: Beautiful hand-crafted SVG icons

### Infrastructure
- **Azure AD B2C**: Authentication (optional, can be enabled later)
- **Azure SQL Database**: Production database
- **Azure App Service**: Hosting
- **Bicep**: Infrastructure as Code

## Project Structure

```
TaskManager/
├── api/                          # Backend .NET API
│   └── TaskMgr.Api/
│       ├── Application/          # Application layer (DTOs, Services, Validators)
│       ├── Controllers/          # API Controllers
│       ├── Data/                 # Database context and migrations
│       ├── Domain/               # Domain layer (Entities, Interfaces, Enums)
│       └── Infrastructure/       # Infrastructure layer (Repositories, Services)
│
├── frontend/                     # Frontend Vue 3 application
│   ├── src/
│   │   ├── assets/              # Static assets and styles
│   │   ├── auth/                # Authentication configuration
│   │   ├── components/          # Reusable Vue components
│   │   ├── composables/         # Vue composables (hooks)
│   │   ├── layouts/             # Layout components
│   │   ├── pages/               # Page components
│   │   ├── router/              # Vue Router configuration
│   │   ├── stores/              # Pinia stores
│   │   └── types/               # TypeScript type definitions
│   └── package.json
│
└── infrastructure/               # Azure infrastructure (Bicep templates)
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (for development)
- [Git](https://git-scm.com/)

### Local Development Setup

#### 1. Clone the Repository

```bash
git clone <repository-url>
cd TaskManager
```

#### 2. Backend Setup

```bash
# Navigate to API project
cd api/TaskMgr.Api

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# (Optional) Seed test data
sqlcmd -S "(localdb)\mssqllocaldb" -d TaskMgrDb -i Data/seed-data.sql

# Run the API
dotnet run
```

The API will be available at:
- HTTP: http://localhost:65454
- HTTPS: https://localhost:65453

Swagger documentation: http://localhost:65454/swagger

#### 3. Frontend Setup

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Create environment file
cp .env.example .env

# Update .env with your settings:
# VITE_API_BASE_URL=http://localhost:65454

# Run development server
npm run dev
```

The frontend will be available at http://localhost:3001

### Database Setup

The project uses SQL Server LocalDB for development. The connection string is configured in `appsettings.Development.json`.

#### Create Database and Apply Migrations

```bash
cd api/TaskMgr.Api
dotnet ef database update
```

#### Seed Test Data

The `Data/seed-data.sql` script creates:
- 3 test users (John Doe, Jane Smith, Bob Wilson)
- 4 projects with various statuses
- 18 tasks across different projects

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -d TaskMgrDb -i Data/seed-data.sql
```

### Development Mode (Without Authentication)

For local development, authentication is disabled by default. The application uses a mock user (Bob Wilson) to allow testing without Azure AD B2C setup.

When you're ready to enable authentication:
1. Configure Azure AD B2C tenant
2. Update `.env` with B2C settings
3. Uncomment `[Authorize]` attributes in controllers
4. Update `GetCurrentUserId()` methods to remove mock user logic
5. Uncomment authentication initialization in `App.vue` and `router/index.ts`

## Testing

### Test Users (Seed Data)

- **John Doe**: john.doe@example.com
- **Jane Smith**: jane.smith@example.com
- **Bob Wilson**: bob.wilson@example.com

### API Endpoints

- **Projects**: `GET /api/projects`
- **Tasks**: `GET /api/tasks`
- **Dashboard**: `GET /api/dashboard/stats`
- **Users**: `GET /api/users`

Full API documentation available at `/swagger` when running the backend.

## Building for Production

### Backend

```bash
cd api/TaskMgr.Api
dotnet publish -c Release -o ./publish
```

### Frontend

```bash
cd frontend
npm run build
```

The production build will be in the `dist/` directory.

## Deployment

### Azure Deployment

Infrastructure as Code templates are provided in the `infrastructure/` directory using Azure Bicep.

```bash
cd infrastructure
az deployment group create \
  --resource-group <your-resource-group> \
  --template-file main.bicep \
  --parameters @parameters.json
```

## Contributing

1. Create a feature branch (`git checkout -b feature/amazing-feature`)
2. Commit your changes (`git commit -m 'Add amazing feature'`)
3. Push to the branch (`git push origin feature/amazing-feature`)
4. Open a Pull Request

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

- **Domain Layer**: Core business logic and entities
- **Application Layer**: Use cases, DTOs, and business rules
- **Infrastructure Layer**: Data access, external services
- **Presentation Layer**: API controllers and responses

### Key Patterns

- **Repository Pattern**: Abstraction over data access
- **Unit of Work**: Transaction management
- **CQRS**: Separation of read and write operations
- **Domain Events**: Decoupled domain logic
- **Dependency Injection**: Loose coupling and testability

## License

This project is licensed under the MIT License.

## Roadmap

- [ ] Real-time notifications with SignalR
- [ ] File attachments for tasks
- [ ] Comments and activity feed
- [ ] Gantt chart view
- [ ] Calendar integration
- [ ] Mobile app (React Native)
- [ ] Advanced reporting and analytics
- [ ] Team collaboration features
- [ ] Email notifications
- [ ] Recurring tasks

## Support

For issues, questions, or contributions, please open an issue on GitHub.

---

Built with ❤️ using .NET 8 and Vue 3

# TaskManager API

Una API completa de gestión de tareas construida con .NET 8, Entity Framework Core y Azure AD B2C.

## 🏗️ Arquitectura

- **Domain-Driven Design (DDD)** con separación de capas
- **Repository Pattern** y Unit of Work
- **CQRS** preparado para escalabilidad
- **AutoMapper** para mapeo de DTOs
- **FluentValidation** para validación de datos
- **Serilog** para logging estructurado
- **Swagger/OpenAPI** para documentación

## 🚀 Configuración Inicial

### Prerequisitos

- .NET 8 SDK
- SQL Server LocalDB o SQL Server
- Azure AD B2C tenant (opcional para desarrollo)

### 1. Instalar Entity Framework Tools

```bash
dotnet tool install --global dotnet-ef
```

### 2. Configurar Base de Datos

Crear la migración inicial:
```bash
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
```

Aplicar migraciones:
```bash
dotnet ef database update
```

### 3. Configurar Azure AD B2C

Actualizar `appsettings.json`:
```json
{
  "AzureAdB2C": {
    "Instance": "https://tu-tenant.b2clogin.com/",
    "Domain": "tu-tenant.onmicrosoft.com",
    "TenantId": "tu-tenant-id",
    "ClientId": "tu-api-client-id",
    "SignUpSignInPolicyId": "B2C_1_signupsignin",
    "Audience": "tu-api-client-id"
  }
}
```

### 4. Ejecutar la API

```bash
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## 📁 Estructura del Proyecto

```
TaskMgr.Api/
├── Domain/                 # Capa de dominio
│   ├── Entities/          # Entidades del dominio
│   ├── Enums/            # Enumeraciones
│   └── Interfaces/       # Interfaces del dominio
├── Application/           # Capa de aplicación
│   ├── DTOs/             # Data Transfer Objects
│   ├── Services/         # Servicios de aplicación
│   ├── Validators/       # Validadores FluentValidation
│   └── Mappings/         # Perfiles de AutoMapper
├── Infrastructure/        # Capa de infraestructura
│   ├── Persistence/      # Implementaciones de repositorio
│   └── Services/         # Servicios de infraestructura
├── Controllers/          # Controladores Web API
├── Middleware/           # Middleware personalizado
├── Extensions/           # Métodos de extensión
├── Data/                # Contexto de EF y migraciones
└── Filters/             # Filtros de acción
```

## 🔐 Autenticación

La API utiliza Azure AD B2C para autenticación. Los endpoints requieren un token JWT válido.

### Headers requeridos:
```
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

## 📊 Endpoints Principales

### Usuarios
- `GET /api/users/me` - Perfil del usuario actual
- `PUT /api/users/me` - Actualizar perfil
- `POST /api/users/initialize` - Inicializar usuario desde token

### Proyectos
- `GET /api/projects` - Listar proyectos
- `POST /api/projects` - Crear proyecto
- `GET /api/projects/{id}` - Obtener proyecto específico
- `PUT /api/projects/{id}` - Actualizar proyecto
- `DELETE /api/projects/{id}` - Eliminar proyecto

### Tareas
- `GET /api/tasks` - Listar tareas (con filtros)
- `POST /api/tasks` - Crear tarea
- `GET /api/tasks/{id}` - Obtener tarea específica
- `PUT /api/tasks/{id}` - Actualizar tarea
- `DELETE /api/tasks/{id}` - Eliminar tarea
- `POST /api/tasks/{id}/time` - Registrar tiempo trabajado

### Dashboard
- `GET /api/dashboard/stats` - Estadísticas del dashboard
- `GET /api/dashboard/activity` - Actividad reciente
- `GET /api/dashboard/trends` - Tendencias de productividad

## 🏥 Health Checks

La API incluye health checks en:
- `/health` - Estado general
- `/health/ready` - Estado de preparación
- `/health/live` - Estado de vida

## 📝 Logging

Logs estructurados con Serilog:
- Console (desarrollo)
- Archivos rotativos (producción)
- Application Insights (Azure)

## 🔧 Variables de Entorno

Para desarrollo local, crear `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskMgrDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

## 🐳 Docker

```bash
# Construir imagen
docker build -t taskmanager-api .

# Ejecutar contenedor
docker run -p 5000:80 taskmanager-api
```

## 🧪 Testing

```bash
# Ejecutar tests unitarios
dotnet test

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📈 Monitoring

- **Métricas**: OpenTelemetry + Application Insights
- **Logs**: Serilog + Azure Monitor
- **Health**: Health checks integrados
- **Performance**: Request/Response timing

## 🔒 Seguridad

- **HTTPS** obligatorio en producción
- **CORS** configurado para orígenes específicos
- **Security headers** automáticos
- **Input validation** con FluentValidation
- **SQL injection** prevención con EF Core
- **Authentication** con Azure AD B2C

## 🚀 Deployment

### Azure App Service

1. Configurar Azure SQL Database
2. Configurar App Service
3. Configurar variables de entorno
4. Deploy desde GitHub Actions

### Infraestructura como Código

Usar el archivo `infra/main.bicep` para crear recursos en Azure.

## 📞 Soporte

Para problemas o preguntas:
1. Revisar logs en `logs/taskmanager-*.log`
2. Verificar health checks
3. Consultar documentación Swagger
4. Revisar configuración de Azure AD B2C

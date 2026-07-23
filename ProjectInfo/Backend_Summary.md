# WFT.Infra Project Workspace Summary

This document summarizes the repository for use by an external AI assistant. It is based on the current workspace contents and project files as audited on July 23, 2026.

## 1. Tech Stack and Versions

### Main Application

| Area | Technology | Version / Target |
|---|---|---|
| Language | C# | Nullable reference types enabled |
| Runtime | .NET | `net10.0` |
| Main framework | ASP.NET Core Web API | .NET 10 |
| API style | Controller-based REST API | ASP.NET Core MVC controllers |
| ORM | Entity Framework Core | `9.0.4` |
| Database provider | EF Core SQL Server provider | `9.0.4` |
| Main database | Microsoft SQL Server | Local database name: `WFTInfraDb` |
| Authentication | JWT Bearer authentication | ASP.NET Core JwtBearer `9.0.4` |
| API documentation | Swagger / OpenAPI | Swashbuckle.AspNetCore `6.5.0` |
| Object mapping | AutoMapper | `16.2.0` |
| Mediator pattern | MediatR | `14.2.0` |
| Password hashing | BCrypt.Net-Next | `4.2.0` |
| Primary test framework | xUnit | `2.9.3` |
| Mocking | Moq | `4.20.70` |

The executable entry point is `src/WFT.Infra.WebApi`. All seven projects included in `WFT.Infra.sln` currently target .NET 10.

### Database Details

- The application uses `Microsoft.EntityFrameworkCore.SqlServer`.
- `ApplicationDbContext` is the main EF Core context.
- The default local connection targets SQL Server database `WFTInfraDb`.
- Windows integrated authentication is used by the committed local connection string through `Trusted_Connection=True`.
- EF Core migrations are stored under `src/WFT.Infra.Infrastructure/Migrations`.
- Pending migrations are automatically applied during API startup through `DatabaseInitializer`.
- The data model includes countries, users, roles, permissions, refresh tokens, RBAC junction tables, and ABAC policy/attribute tables.

## 2. Project Architecture

The repository follows a layered Clean Architecture-style structure:

```text
Web API
   |
Bootstrapper
   |-------------------|
Application       Infrastructure
   |                  |
Application.Contracts |
   |                  |
   +------- Core -----+
```

### Layer Responsibilities

#### `WFT.Infra.WebApi`

The ASP.NET Core host and presentation layer.

- Contains `Program.cs`, API controllers, middleware, and HTTP launch profiles.
- Configures controllers and JSON serialization.
- Configures JWT authentication and authorization.
- Configures Swagger/OpenAPI.
- Loads application configuration.
- Runs initial data seeding.
- Applies EF Core migrations at startup.
- References only the Bootstrapper project directly.

Key folders:

- `Controllers`: REST endpoints for authentication, users, roles, permissions, countries, RBAC, and ABAC.
- `CustomConfig`: API response envelope/result types and response middleware.
- `Properties`: Local launch profiles and development environment selection.

#### `WFT.Infra.Bootstrapper`

The composition root between the host and implementation layers.

- Exposes `AddProjectDependencies`.
- Registers the Application layer.
- Registers the Infrastructure layer.
- Allows the Web API to avoid directly referencing all lower-level projects.

#### `WFT.Infra.Application`

The use-case and business-service layer.

- Contains application services for authentication, tokens, countries, users, roles, permissions, RBAC, and ABAC.
- Contains dependency injection registrations.
- Contains AutoMapper profiles.
- Contains MediatR command registration.
- Contains password hashing and work-context helpers.
- Contains initial reference-data seeding.
- Implements interfaces defined in `Application.Contracts`.

Key folders:

- `Services`: Core application services.
- `Services/UserManagment`: User, role, permission, policy, attribute, and ABAC services.
- `Mapping`: AutoMapper profiles.
- `Permissions`: Custom permission authorization components.
- `InitialData`: Database seed logic.
- `Helper`: Password and JWT helpers.
- `Commands`: MediatR command types.

> Note: The repository consistently uses the directory/namespace spelling `UserManagment`, rather than `UserManagement`.

#### `WFT.Infra.Application.Contracts`

The shared contracts and boundary-model layer.

- Defines service interfaces.
- Defines repository interfaces.
- Defines DTOs for API and application boundaries.
- Defines pagination and API envelope models.
- Defines JWT settings.
- Has no NuGet package dependencies or project references.

Key folders:

- `DTOs`: General DTOs.
- `DTOs/UserManagment`: Authentication, RBAC, and ABAC DTOs.
- `Interfaces`: General service abstractions.
- `Interfaces/UserManagment`: User, role, permission, and ABAC abstractions.
- `Repositories`: Generic and user repository contracts.
- `Models`: Pagination, token, metadata, and envelope models.
- `Settings`: Strongly typed JWT configuration.

#### `WFT.Infra.Core`

The domain/entity layer.

- Contains domain entities and base entity behavior.
- Contains RBAC and ABAC entity models.
- Has no NuGet dependencies or project references.
- Is referenced by Application and Infrastructure.

Key folders:

- `Entities`: Base entities, countries, refresh tokens, and other domain models.
- `Entities/UserManagment`: Users, roles, permissions, junction entities, policies, conditions, and attribute entities.

#### `WFT.Infra.Infrastructure`

The persistence and technical implementation layer.

- Implements generic and user repositories.
- Owns `ApplicationDbContext`.
- Configures EF Core entity mappings.
- Configures SQL Server.
- Implements JWT token generation.
- Contains database migrations.
- Implements automatic database migration initialization.

Key folders:

- `Data`: EF Core context and design-time context factory.
- `Configurations`: Fluent API entity configurations.
- `Repositories`: Repository implementations.
- `Security`: Infrastructure JWT implementation.
- `Migrations`: EF Core migration history and model snapshot.

#### `WFT.Infra.Test`

The test project included in the main solution.

- Uses xUnit as the main test framework.
- Also references MSTest.TestFramework.
- Uses EF Core InMemory and Moq.
- Contains pagination, JSON API format, and RBAC verification tests.

#### `WFT.Test`

A second, legacy-looking MSTest project exists under `src/WFT.Test`, but it is not included in `WFT.Infra.sln`.

- It references `../../WFT.Bussines/WFT.Bussines.csproj`, which is outside this repository/workspace.
- It still targets `net9.0`, unlike the seven .NET 10 projects in the main solution.
- It should not be treated as part of the main build unless that external project is restored and the test project is intentionally added to the solution.

### Project Reference Flow

```text
WFT.Infra.WebApi
└── WFT.Infra.Bootstrapper
    ├── WFT.Infra.Application
    │   ├── WFT.Infra.Application.Contracts
    │   └── WFT.Infra.Core
    ├── WFT.Infra.Infrastructure
    │   ├── WFT.Infra.Application.Contracts
    │   └── WFT.Infra.Core
    └── WFT.Infra.Application.Contracts

WFT.Infra.Test
├── WFT.Infra.Application
├── WFT.Infra.Infrastructure
└── WFT.Infra.Core
```

## 3. Core Dependencies

### Web API

| Package | Version | Purpose |
|---|---:|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | `9.0.4` | JWT bearer authentication |
| `Microsoft.EntityFrameworkCore.Design` | `9.0.4` | EF Core design-time support |
| `Swashbuckle.AspNetCore` | `6.5.0` | Swagger/OpenAPI generation and UI |

### Application

| Package | Version | Purpose |
|---|---:|---|
| `AutoMapper` | `16.2.0` | Entity/DTO object mapping |
| `BCrypt.Net-Next` | `4.2.0` | Password hashing and verification |
| `MediatR` | `14.2.0` | Mediator/command registration |
| `Microsoft.AspNetCore.Authorization` | `10.0.10` | Authorization abstractions and services |
| `Microsoft.AspNetCore.Http.Abstractions` | `2.3.11` | HTTP context abstractions |
| `Microsoft.Extensions.Configuration` | `10.0.10` | Configuration abstractions |
| `Microsoft.Extensions.Configuration.Json` | `10.0.10` | JSON configuration provider |
| `System.Configuration.ConfigurationManager` | `10.0.10` | Legacy-style configuration APIs |
| `System.IdentityModel.Tokens.Jwt` | `8.21.0` | JWT creation and processing |

### Infrastructure

| Package | Version | Purpose |
|---|---:|---|
| `Microsoft.EntityFrameworkCore` | `9.0.4` | ORM core |
| `Microsoft.EntityFrameworkCore.SqlServer` | `9.0.4` | SQL Server database provider |
| `Microsoft.EntityFrameworkCore.Design` | `9.0.4` | Migration/design-time tooling |
| `Microsoft.EntityFrameworkCore.Tools` | `9.0.4` | EF Core command tooling |
| `Microsoft.Extensions.Configuration` | `9.0.4` | Configuration abstractions |
| `Microsoft.Extensions.Configuration.FileExtensions` | `9.0.4` | File-based configuration support |
| `Microsoft.Extensions.Configuration.Json` | `9.0.4` | JSON settings support |
| `Microsoft.Extensions.Configuration.EnvironmentVariables` | `9.0.4` | Environment-variable overrides |
| `Microsoft.Extensions.Configuration.UserSecrets` | `9.0.4` | User Secrets provider support |

### Tests

| Package | Version | Purpose |
|---|---:|---|
| `xunit` | `2.9.3` | Unit testing |
| `xunit.runner.visualstudio` | `2.8.2` | Visual Studio/.NET test discovery |
| `Microsoft.NET.Test.Sdk` | `17.11.1` | Test host |
| `Microsoft.EntityFrameworkCore.InMemory` | `9.0.0` | In-memory EF Core test provider |
| `Moq` | `4.20.70` | Mocking |
| `MSTest.TestFramework` | `4.0.0` | Additional MSTest APIs |

### Dependency Version Caution

The main solution now targets .NET 10, but several runtime-facing packages remain on the .NET 9 line:

- Entity Framework Core, SQL Server, design tools, and configuration packages remain at `9.0.4`.
- Web API JWT Bearer authentication remains at `9.0.4`.
- EF Core InMemory in tests remains at `9.0.0`.
- The Application project uses several `10.0.10` Microsoft packages, which align more closely with the new target.
- `Microsoft.AspNetCore.Http.Abstractions` remains at the substantially older `2.3.11`.

The target-framework change is therefore ahead of the EF Core and JwtBearer package upgrades. Compatibility and restore/build behavior should be verified before additional framework or package changes.

## 4. Current API Surface

All controllers inherit from `BaseController`, which applies:

```text
[Authorize]
api/app/v1/wft/[controller]
```

`AuthController` overrides the authorization requirement with `[AllowAnonymous]` at controller level.

Current backend controllers:

| Controller | Primary responsibility |
|---|---|
| `AuthController` | Register, login, logout, and refresh token |
| `UsersController` | User CRUD and paged listing |
| `UserPasswordController` | Set and update user passwords |
| `RolesController` | Role CRUD and paged listing |
| `PermissionsController` | Permission CRUD and paged listing |
| `CountryController` | Country CRUD and paged listing |
| `AttributeDefinitionsController` | ABAC attribute definition CRUD and lookup |
| `AttributeGroupsController` | Attribute group CRUD, ordering, and membership |
| `ConditionOperatorsController` | ABAC condition operator CRUD and lookup |
| `PolicyRulesController` | Policy CRUD and role/permission queries |
| `RolePolicyAssignmentController` | Assign and remove policies from roles |
| `ABACController` | Access evaluation, attributes, policy validation, and statistics |

Notable route facts:

- There is no `RolePermissionController` in the current backend controller directory.
- There is no `UserAttributesController`; user-attribute operations currently live under `ABACController`.
- The role-policy controller route is singular by convention: `/rolepolicyassignment/...`.
- The current controllers do not expose generic `/All` endpoints for attribute definitions, policy rules, condition operators, or attribute groups.
- These details matter because parts of the adjacent React workspace currently call different route names.

## 5. Simplified Project Structure Tree

```text
WFTInfra/
├── WFT.Infra.sln
├── README.md
├── PROJECT_WORKSPACE_SUMMARY.md
├── .github/
├── TestResults/
├── *.md                              # Migration, RBAC/ABAC, pagination, and API notes
└── src/
    ├── WFT.Infra.Core/
    │   ├── Entities/
    │   │   └── UserManagment/
    │   └── WFT.Infra.Core.csproj
    │
    ├── WFT.Infra.Application.Contracts/
    │   ├── DTOs/
    │   │   └── UserManagment/
    │   ├── Interfaces/
    │   │   └── UserManagment/
    │   ├── Models/
    │   ├── Repositories/
    │   ├── Settings/
    │   └── WFT.Infra.Application.Contracts.csproj
    │
    ├── WFT.Infra.Application/
    │   ├── Commands/
    │   ├── Helper/
    │   ├── InitialData/
    │   ├── Mapping/
    │   │   └── Profiles/
    │   ├── Permissions/
    │   ├── Services/
    │   │   └── UserManagment/
    │   ├── DependencyInjection.cs
    │   └── WFT.Infra.Application.csproj
    │
    ├── WFT.Infra.Infrastructure/
    │   ├── Configurations/
    │   ├── Data/
    │   │   ├── ApplicationDbContext.cs
    │   │   └── ApplicationDbContextFactory.cs
    │   ├── Migrations/
    │   ├── Repositories/
    │   ├── Security/
    │   ├── DatabaseInitializer.cs
    │   ├── DependencyInjection.cs
    │   └── WFT.Infra.Infrastructure.csproj
    │
    ├── WFT.Infra.Bootstrapper/
    │   ├── DependencyInjection.cs
    │   └── WFT.Infra.Bootstrapper.csproj
    │
    ├── WFT.Infra.WebApi/
    │   ├── Controllers/
    │   ├── CustomConfig/
    │   ├── Properties/
    │   │   └── launchSettings.json
    │   ├── Program.cs
    │   ├── appsettings.json
    │   ├── appsettings.Development.json
    │   ├── appsettings.Production.json
    │   └── WFT.Infra.WebApi.csproj
    │
    ├── WFT.Infra.Test/
    │   ├── Helpers/
    │   ├── Pagination/
    │   ├── RBAC/
    │   └── WFT.Infra.Test.csproj
    │
    └── WFT.Test/                      # Legacy/external-reference test project
        └── WFT.Test.csproj
```

## 6. Configuration Pattern

### Runtime Configuration Loading

The Web API uses the standard ASP.NET Core configuration system. `Program.cs` explicitly adds providers in this order:

1. `appsettings.json` — required base configuration.
2. `appsettings.{Environment}.json` — optional environment-specific configuration.
3. Environment variables — final override layer.

Later providers override earlier providers. For example, an environment variable overrides the same key from either JSON file.

The active environment is supplied by `ASPNETCORE_ENVIRONMENT`. Both local launch profiles set it to `Development`.

### Connection Strings

The main key is:

```text
ConnectionStrings:DefaultConnection
```

It is read through:

```csharp
configuration.GetConnectionString("DefaultConnection")
```

and passed to:

```csharp
options.UseSqlServer(connectionString)
```

The equivalent environment-variable name is:

```text
ConnectionStrings__DefaultConnection
```

Double underscores map environment-variable segments to configuration colons.

Examples:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
```

PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
```

### JWT Configuration

JWT settings are stored under:

```text
JwtSettings
```

with these keys:

- `Secret`
- `Issuer`
- `Audience`
- `ExpiryInMinutes`

They are bound to `WFT.Infra.Application.Contracts.Settings.JwtSettings` through the options pattern and are also read directly when configuring JWT bearer token validation.

Environment-variable equivalents are:

```text
JwtSettings__Secret
JwtSettings__Issuer
JwtSettings__Audience
JwtSettings__ExpiryInMinutes
```

Production values in `appsettings.Production.json` are placeholders and must be replaced or overridden during deployment.

### Environment Files

| File | Role |
|---|---|
| `src/WFT.Infra.WebApi/appsettings.json` | Base settings and local defaults |
| `src/WFT.Infra.WebApi/appsettings.Development.json` | Development logging, database, and JWT overrides |
| `src/WFT.Infra.WebApi/appsettings.Production.json` | Production placeholders and stricter logging |
| `src/WFT.Infra.WebApi/Properties/launchSettings.json` | Local ports and `ASPNETCORE_ENVIRONMENT=Development` |
| `src/WFT.Infra.Infrastructure/appsettings.json` | Additional infrastructure-local connection file; not used by normal Web API runtime startup |

### Design-Time EF Core Configuration

`ApplicationDbContextFactory` supports EF Core CLI/design-time operations:

1. Reads `ASPNETCORE_ENVIRONMENT`, defaulting to `Development`.
2. Uses the current working directory as its configuration base path.
3. Loads `appsettings.json`.
4. Loads the matching optional environment JSON file.
5. Loads environment variables last.
6. Requires `ConnectionStrings:DefaultConnection`.

Because the factory uses `Directory.GetCurrentDirectory()`, EF commands must run from a directory where the expected appsettings files are available, or be invoked with suitable startup/project arguments and environment overrides.

Typical migration command from the repository root:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/WFT.Infra.Infrastructure \
  --startup-project src/WFT.Infra.WebApi
```

Typical database update:

```bash
dotnet ef database update \
  --project src/WFT.Infra.Infrastructure \
  --startup-project src/WFT.Infra.WebApi
```

### User Secrets and Sensitive Values

The Web API project defines a `UserSecretsId`, and the Infrastructure project references the User Secrets configuration package. ASP.NET Core's default builder can load user secrets in Development for an entry-point project with a `UserSecretsId`.

Recommended local secret commands:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>" \
  --project src/WFT.Infra.WebApi

dotnet user-secrets set "JwtSettings:Secret" "<minimum-32-character-secret>" \
  --project src/WFT.Infra.WebApi
```

Secrets and real production connection strings should not be committed to the JSON configuration files. Production deployments should preferably inject them through environment variables, a secret store, or the hosting platform's configuration system.

## Additional Operational Notes

- Swagger UI is enabled only when the application environment is `Development`.
- Swagger UI is hosted at the application root in Development.
- JSON responses use camelCase, indented output, and cycle-reference ignoring.
- A custom `WFTResponseMiddleware` wraps or standardizes API responses.
- JWT authentication runs before authorization middleware.
- `InitialDataSeeder.SeedAsync()` runs during startup.
- `DatabaseInitializer.Initialize()` calls `Database.Migrate()` during startup.
- Startup therefore requires a reachable SQL Server and sufficient database permissions to query and apply migrations.
- The solution has no checked-in `global.json`, so no exact .NET SDK patch version is pinned.
- No central `Directory.Packages.props` or `Directory.Build.props` file is present; package versions are maintained separately in each project file.
- The active solution projects target .NET 10, but some package versions and older documentation still reflect the previous .NET 9 baseline.
- A build was not performed during this documentation audit because the current shell environment does not expose the `dotnet` CLI.

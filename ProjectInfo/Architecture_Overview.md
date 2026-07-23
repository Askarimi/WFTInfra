# WFT.Infra Architecture Overview

## 1. System Overview

WFT.Infra is a full-stack access-management platform composed of two independent applications:

- A .NET 9 ASP.NET Core backend following a layered Clean Architecture approach.
- A React 19 and TypeScript frontend built with Mantine, Redux Toolkit, React Router, Axios, and Vite.

The platform supports:

- Authentication with JWT access and refresh tokens.
- User, role, and permission management.
- Role-based access control (RBAC).
- Attribute-based access control (ABAC).
- Policy, attribute, condition, and role-policy administration.
- Backend-driven pagination and filtering.
- Persian/Farsi right-to-left administration screens.

```text
┌──────────────────────────────────────────────────────────────┐
│                 React 19 Administration UI                   │
│      Mantine 6 · Redux Toolkit · React Router · Axios        │
└──────────────────────────────┬───────────────────────────────┘
                               │ HTTPS / JSON / JWT
                               │ /api/app/v1/wft
┌──────────────────────────────▼───────────────────────────────┐
│                    ASP.NET Core Web API                      │
│          Controllers · Middleware · Authentication           │
├──────────────────────────────────────────────────────────────┤
│                         Application                          │
│       Use Cases · Services · RBAC/ABAC · Mapping · Seed      │
├──────────────────────────────────────────────────────────────┤
│                       Infrastructure                         │
│     EF Core · SQL Server · Repositories · JWT · Migrations   │
├──────────────────────────────────────────────────────────────┤
│                            Core                              │
│                  Domain Entities and Rules                   │
└──────────────────────────────┬───────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────┐
│                     Microsoft SQL Server                     │
└──────────────────────────────────────────────────────────────┘
```

## 2. Backend Architecture

### Technology

| Area | Technology |
|---|---|
| Runtime | .NET 9 |
| Host | ASP.NET Core Web API |
| Language | C# |
| ORM | Entity Framework Core 9.0.4 |
| Database | Microsoft SQL Server |
| Authentication | JWT Bearer |
| API documentation | Swagger/OpenAPI |
| Mapping | AutoMapper |
| Application messaging | MediatR |
| Password hashing | BCrypt |

### Clean Architecture Layers

#### Core

Project: `src/WFT.Infra.Core`

The Core layer contains the domain entities and remains independent of framework and persistence implementations.

Primary entity groups:

- Base entities and audit timestamps.
- Countries and refresh tokens.
- Users, passwords, roles, and permissions.
- User-role and role-permission junctions.
- Attribute groups, definitions, and values.
- ABAC policy rules, policy conditions, and condition operators.
- Role-policy assignments.

#### Application Contracts

Project: `src/WFT.Infra.Application.Contracts`

This layer defines boundaries shared between the application and external layers:

- Service interfaces.
- Repository abstractions.
- DTOs.
- API response and pagination models.
- JWT configuration models.
- RBAC and ABAC request/response contracts.

Keeping these definitions separate allows controllers and infrastructure implementations to depend on abstractions rather than concrete services.

#### Application

Project: `src/WFT.Infra.Application`

The Application layer implements business use cases:

- Authentication, login, logout, and refresh-token workflows.
- User and password management.
- Role and permission management.
- RBAC authorization.
- ABAC policy evaluation and administration.
- Country/reference-data operations.
- DTO/entity mapping.
- Initial data seeding.
- Work-context and password-hashing helpers.

Dependency injection is registered through `AddApplication()`.

#### Infrastructure

Project: `src/WFT.Infra.Infrastructure`

The Infrastructure layer provides technical implementations:

- `ApplicationDbContext`.
- SQL Server configuration through `UseSqlServer`.
- EF Core entity configurations.
- Generic and user repositories.
- EF Core migrations.
- Database migration initialization.
- JWT token generation.

Infrastructure implements interfaces defined by Application Contracts and depends on Core entities.

#### Bootstrapper

Project: `src/WFT.Infra.Bootstrapper`

The Bootstrapper is the composition bridge used by the Web API:

```text
AddProjectDependencies(configuration)
├── AddApplication()
└── AddInfrastructure(configuration)
```

This keeps the Web API entry project focused on host-level configuration.

#### Web API

Project: `src/WFT.Infra.WebApi`

The Web API is the presentation and runtime layer:

- Defines API controllers.
- Configures JWT authentication and authorization.
- Configures Swagger/OpenAPI.
- Configures camelCase JSON serialization.
- Runs custom response middleware.
- Loads configuration and environment overrides.
- Seeds initial data.
- Applies pending database migrations.

### Backend Dependency Direction

```text
WebApi
└── Bootstrapper
    ├── Application
    │   ├── Application.Contracts
    │   └── Core
    └── Infrastructure
        ├── Application.Contracts
        └── Core
```

The Core and Application Contracts projects do not depend on Infrastructure or Web API.

## 3. Frontend Architecture

### Technology

| Area | Technology |
|---|---|
| UI framework | React 19.1 |
| Language | TypeScript 5.8 |
| Build tool | Vite 6 |
| Component library | Mantine 6 |
| Global state | Redux Toolkit |
| Routing | React Router 7 |
| HTTP client | Axios |
| Table framework | Mantine React Table |
| Icons | Tabler Icons |
| Styling direction | Persian RTL |
| Package manager | Yarn Classic |

### Frontend Layout

```text
src/
├── App.tsx
│   ├── Mantine and color-scheme providers
│   ├── RTL Emotion cache
│   ├── Authentication rehydration
│   └── Application routes
├── main.tsx
│   └── React and Redux entry point
├── components/
│   ├── AppShell/layout components
│   ├── DataTable/
│   ├── PermissionGuard/
│   └── shared UI components
├── features/
│   └── auth/
│       ├── Redux store and slice
│       ├── authentication services
│       └── login UI
├── pages/
│   ├── Dashboard
│   ├── Users
│   ├── Roles
│   ├── Permissions
│   └── ABAC administration pages
├── providers/
│   └── HttpProvider/
│       ├── apiClient.ts
│       ├── httpService.ts
│       └── tokenManager.ts
├── services/
│   ├── user.service.ts
│   ├── role.service.ts
│   ├── permission.service.ts
│   ├── rolePermission.service.ts
│   └── abac.service.ts
├── assets/
│   ├── fonts/
│   └── images/
└── hooks.ts
```

### Application Shell

The authenticated user interface uses Mantine's application-shell components:

- Header.
- Responsive navigation/sidebar.
- Burger menu for smaller viewports.
- Nested content rendered through React Router's `Outlet`.
- Light and dark themes persisted in local storage.
- Persian RTL layout and fonts.

The route hierarchy is:

```text
BrowserRouter
├── Public routes
│   └── /login
└── PrivateRoute
    └── AppShellLayout
        ├── /dashboard
        ├── /users
        ├── /roles
        ├── /permissions
        ├── /attributes
        ├── /attribute-groups
        ├── /policies
        ├── /operators
        ├── /assignments
        ├── /evaluation
        ├── /user-attributes
        └── /settings
```

### State Management

Redux Toolkit manages global authentication state:

- Authenticated user.
- Access token.
- Loading and error states.
- Authentication status.
- Initial local-storage rehydration status.

Authentication values are persisted in browser local storage. Feature pages otherwise primarily use component-local React state.

Theme preference is managed separately through a local-storage hook rather than Redux.

### Mantine UI Pattern

Mantine provides the primary layout and component system:

- `MantineProvider` and `ColorSchemeProvider`.
- `AppShell`, `Navbar`, `Header`, `NavLink`, and `Burger`.
- Forms through `@mantine/form`.
- Modals through `@mantine/modals`.
- Notifications through `@mantine/notifications`.
- Reusable data tables through `mantine-react-table`.
- Tabler icons.

The UI does not use Tailwind, Material UI, CSS Modules, or Styled Components. Styling is primarily implemented through Mantine props, theme values, style callbacks, and inline style objects.

## 4. Frontend-to-Backend Integration

### Request Flow

```text
React page/component
    ↓
Domain service
    ↓
HttpService
    ↓
Axios instance
    ↓
Vite development proxy
    ↓
ASP.NET Core controller
    ↓
Application service
    ↓
Repository / ApplicationDbContext
    ↓
SQL Server
```

### API Addressing

The frontend uses:

```text
VITE_API_BASE_URL=/api/app/v1/wft
```

During development, Vite proxies the API path to:

```text
https://localhost:7041
```

This matches the backend HTTPS launch profile.

### Authentication Flow

1. The user submits login credentials from the React login page.
2. The authentication service calls the backend auth endpoint.
3. The access token and user data are stored in Redux and local storage.
4. The Axios request interceptor adds `Authorization: Bearer <token>`.
5. `PrivateRoute` prevents unauthenticated access to protected pages.
6. UI permission guards hide actions unavailable to the user.
7. The backend remains the authoritative authorization boundary.

### Token Refresh

The Axios response interceptor handles expired access tokens:

1. An API request receives HTTP 401.
2. The client starts one refresh request.
3. Concurrent failed requests wait in a queue.
4. A successful refresh stores the new token.
5. Queued requests are retried.
6. A failed refresh clears authentication and redirects to `/login`.

### API Response Contract

The frontend expects a response envelope similar to:

```json
{
  "success": true,
  "data": {},
  "message": null,
  "error": null,
  "logId": null,
  "meta": {}
}
```

Paged endpoints additionally expose pagination metadata. Changes to backend envelopes or pagination models must be coordinated with `HttpService` and the frontend domain services.

## 5. Authorization Architecture

### Backend Enforcement

The backend owns authoritative authorization decisions:

- JWT validation establishes the caller identity.
- RBAC checks evaluate assigned roles and permissions.
- ABAC checks evaluate attributes, policies, conditions, resources, and actions.
- Controllers and application services should reject unauthorized operations even when the UI hides them.

### Frontend Presentation

The frontend uses authorization information to improve user experience:

- `PrivateRoute` controls authenticated route access.
- `PermissionChecker` evaluates roles and permission names.
- `PermissionGuard` conditionally renders actions and UI sections.
- ABAC-specific checks call the backend evaluation endpoint.

Frontend permission checks are presentation controls, not security boundaries.

## 6. Configuration Boundaries

### Backend

Backend configuration is loaded from:

1. `appsettings.json`.
2. `appsettings.{Environment}.json`.
3. Environment variables.

Important environment variables:

```text
ASPNETCORE_ENVIRONMENT
ConnectionStrings__DefaultConnection
JwtSettings__Secret
JwtSettings__Issuer
JwtSettings__Audience
JwtSettings__ExpiryInMinutes
```

### Frontend

Frontend build/runtime configuration uses Vite variables:

```text
VITE_API_BASE_URL
```

Only variables prefixed with `VITE_` are exposed to browser code. Secrets must never be stored in frontend environment files because they become part of the client bundle.

## 7. Deployment Layout

A typical deployment separates the browser application, API, and database:

```text
Browser
   ↓ HTTPS
React static application
   ↓ HTTPS / reverse proxy
ASP.NET Core Web API
   ↓ SQL connection
Microsoft SQL Server
```

Recommended deployment responsibilities:

- Build the React application to static assets using `yarn build`.
- Serve frontend assets through a web server, CDN, or reverse proxy.
- Run the .NET Web API as a separate process/container.
- Route the frontend API prefix to the backend.
- Inject backend connection strings and JWT configuration securely.
- Configure CORS if frontend and backend use different origins without a shared reverse proxy.
- Grant the backend database account migration permissions only if automatic startup migrations remain enabled.

## 8. Important Cross-Project Considerations

- Backend contract changes must be reflected in frontend TypeScript models and services.
- Pagination naming and metadata must remain synchronized.
- The Axios POST helper currently unwraps responses differently from other verbs; standardize it carefully across all callers.
- JWT refresh behavior depends on the exact backend refresh endpoint and response shape.
- The backend uses camelCase JSON serialization, matching normal TypeScript naming conventions.
- The UI is RTL-first; new components must preserve direction, Persian localization, and responsive navigation behavior.
- UI permission guards must never replace backend authorization.
- The backend targets .NET 9 but includes some Microsoft package references from other major framework versions; package upgrades should be reviewed as a coordinated task.
- The backend automatically seeds data and applies migrations on startup, making database availability part of application startup.

## 9. Related Documentation

- `ProjectInfo/Backend_Summary.md` contains the detailed .NET backend workspace analysis.
- `ProjectInfo/Frontend_Summary.md` contains the detailed React frontend workspace analysis.

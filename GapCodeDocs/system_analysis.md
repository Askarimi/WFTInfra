# WFT Infrastructure AI Context / System Prompt

> **Mandatory maintenance rule for all future coding AI sessions:** whenever any code, configuration, migration, test, or API contract is modified in this repository, update this document in the same task. At minimum, update **Current Project State & Progress Log**, and update any architecture, data-flow, endpoint, or guideline section affected by the change. This file is the project handoff memory for Cursor, Claude, Copilot, GPT, GapCode, and any future coding AI.

## 1. System Role & Project Purpose

You are working on **WFT.Infra**, an ASP.NET Core backend infrastructure service focused on authentication, user management, RBAC, ABAC, standardized API responses, pagination, EF Core persistence, and Swagger-documented HTTP APIs.

Primary objective:
- Provide a reusable backend infrastructure API for managing users, roles, permissions, role-permission assignment, attribute-based access control policies, and token-based authentication.
- Combine **RBAC** as the base permission model with **ABAC** policies for contextual/resource/environment decisions.
- Return predictable JSON envelopes and paginated responses that are friendly to frontend clients.

Tech stack:
- **.NET / ASP.NET Core 9** (`net9.0`)
- **Entity Framework Core 9** with SQL Server
- **JWT Bearer authentication**
- **BCrypt.Net-Next** password hashing
- **AutoMapper 15**
- **MediatR 12**
- **Swashbuckle Swagger**
- **xUnit + EF Core InMemory + Moq** in `src/WFT.Infra.Test`
- Mixed Persian and English code comments/messages; keep existing language style when editing nearby code.

Solution:
- `WFT.Infra.sln`
- Main branch seen locally: `usermanagment`
- Current local tree has many pre-existing uncommitted changes. Do not assume a clean working tree.

## 2. Codebase Map (Folder & File Structure)

```text
.
├── WFT.Infra.sln
├── README.md
├── *_SUMMARY.md / *_ANALYSIS*.md / *_GUIDE.md
│   └── Project notes, migration summaries, RBAC/ABAC reports, JSON response notes, setup docs.
├── GapCodeDocs/
│   └── system_analysis.md
├── src/
│   ├── WFT.Infra.Core/
│   │   ├── WFT.Infra.Core.csproj
│   │   └── Entities/
│   │       ├── BaseEntity.cs
│   │       ├── Country.cs
│   │       ├── RefreshToken.cs
│   │       └── UserManagment/
│   │           ├── User.cs
│   │           ├── UserPassword.cs
│   │           ├── Role.cs
│   │           ├── Permission.cs
│   │           ├── UserRole.cs
│   │           ├── RolePermission.cs
│   │           ├── AttributeGroup.cs
│   │           ├── AttributeDefinition.cs
│   │           ├── AttributeValue.cs
│   │           ├── ConditionOperator.cs
│   │           ├── PolicyRule.cs
│   │           ├── PolicyCondition.cs
│   │           └── RolePolicyRule.cs
│   ├── WFT.Infra.Application.Contracts/
│   │   ├── DTOs/
│   │   │   ├── UserManagment/
│   │   │   │   ├── UserDto.cs
│   │   │   │   ├── RoleDto.cs
│   │   │   │   ├── PermissionDto.cs
│   │   │   │   ├── UserLoginDto.cs
│   │   │   │   ├── UserRegisterDto.cs
│   │   │   │   ├── LoginResponseDto.cs
│   │   │   │   ├── ABACEvaluationRequestDto.cs
│   │   │   │   ├── ABACEvaluationResponseDto.cs
│   │   │   │   ├── Attribute*Dto.cs
│   │   │   │   ├── ConditionOperatorDto.cs
│   │   │   │   ├── PolicyRuleDto.cs
│   │   │   │   └── PolicyConditionDto.cs
│   │   │   └── RolePermissionManagement/
│   │   │       ├── CreateRoleDto.cs
│   │   │       ├── UpdateRoleDto.cs
│   │   │       ├── RoleDetailDto.cs
│   │   │       ├── CreatePermissionDto.cs
│   │   │       ├── UpdatePermissionDto.cs
│   │   │       ├── PermissionDetailDto.cs
│   │   │       ├── AssignPermissionsDto.cs
│   │   │       ├── RemovePermissionDto.cs
│   │   │       └── RoleWithPermissionsDto.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IAuthorizationService.cs
│   │   │   ├── IJwtTokenGenerator.cs
│   │   │   ├── IRefreshTokenService.cs
│   │   │   ├── ITokenService.cs
│   │   │   ├── IWorkContext.cs
│   │   │   ├── UserManagment/
│   │   │   └── RolePermissionManagement/
│   │   ├── Models/
│   │   │   ├── ApiEnvelope.cs
│   │   │   ├── Meta.cs
│   │   │   ├── PagedList.cs
│   │   │   ├── PagedQueryRequest.cs
│   │   │   ├── PagedResponse.cs
│   │   │   ├── PagedResult.cs
│   │   │   └── TokenRequest.cs
│   │   ├── Repositories/
│   │   │   ├── IRepository.cs
│   │   │   └── IUserRepository.cs
│   │   └── Settings/
│   │       └── JwtSettings.cs
│   ├── WFT.Infra.Application/
│   │   ├── DependencyInjection.cs
│   │   ├── Helper/
│   │   │   ├── PasswordHasher.cs
│   │   │   └── JwtTokenGenerator.cs
│   │   ├── InitialData/
│   │   │   └── InitialDataSeeder.cs
│   │   ├── Mapping/
│   │   │   ├── BaseProfile.cs
│   │   │   └── Profiles/
│   │   │       ├── ABACProfile.cs
│   │   │       ├── CountryProfile.cs
│   │   │       ├── GlobalEntitesProfile.cs
│   │   │       ├── UserProfile.cs
│   │   │       └── UserManagmentProfile/UserManagmentProfile.cs
│   │   ├── Permissions/
│   │   │   ├── HasPermissionAttribute.cs
│   │   │   ├── PermissionHandler.cs
│   │   │   └── PermissionRequirement.cs
│   │   └── Services/
│   │       ├── AuthService.cs
│   │       ├── AuthorizationService.cs
│   │       ├── CountryService.cs
│   │       ├── RefreshTokenService.cs
│   │       ├── TokenService.cs
│   │       ├── WorkContext.cs
│   │       ├── UserManagment/
│   │       │   ├── LoginService.cs
│   │       │   ├── UserService.cs
│   │       │   ├── RoleService.cs
│   │       │   ├── PermissionService.cs
│   │       │   ├── ABACService.cs
│   │       │   ├── AttributeService.cs
│   │       │   ├── AttributeGroupService.cs
│   │       │   ├── ConditionOperatorService.cs
│   │       │   └── PolicyRuleService.cs
│   │       └── RolePermissionManagement/
│   │           ├── RoleService.cs
│   │           ├── PermissionService.cs
│   │           └── RolePermissionService.cs
│   ├── WFT.Infra.Infrastructure/
│   │   ├── DependencyInjection.cs
│   │   ├── DatabaseInitializer.cs
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── ApplicationDbContextFactory.cs
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── RoleConfiguration.cs
│   │   │   ├── PermissionConfiguration.cs
│   │   │   ├── UserRoleConfiguration.cs
│   │   │   ├── RolePermissionConfiguration.cs
│   │   │   ├── UserPasswordConfiguration.cs
│   │   │   ├── Attribute*Configuration.cs
│   │   │   ├── Policy*Configuration.cs
│   │   │   ├── ConditionOperatorConfiguration.cs
│   │   │   └── RolePolicyRuleConfiguration.cs
│   │   ├── Migrations/
│   │   │   ├── 20250710110908_first-migration.cs
│   │   │   ├── 20250803092452_init-ABAC.cs
│   │   │   ├── 20250803121921_init-ABAC-attribute-groups.cs
│   │   │   ├── 20251010065148_FixRbacJunctionTablesAndConfigurations.cs
│   │   │   ├── 20251010073934_Fix_RBACRelations.cs
│   │   │   └── ApplicationDbContextModelSnapshot.cs
│   │   ├── Repositories/
│   │   │   ├── Repository.cs
│   │   │   └── UserRepository.cs
│   │   └── Security/
│   │       └── JwtTokenGenerator.cs
│   ├── WFT.Infra.Bootstrapper/
│   │   └── DependencyInjection.cs
│   ├── WFT.Infra.WebApi/
│   │   ├── Program.cs
│   │   ├── appsettings*.json
│   │   ├── CustomConfig/
│   │   │   ├── WFTJsonResult.cs
│   │   │   ├── WFTResponseMiddleware.cs
│   │   │   └── MetaData.cs
│   │   └── Controllers/
│   │       ├── BaseController.cs
│   │       ├── AuthController.cs
│   │       ├── UsersController.cs
│   │       ├── RolesController.cs
│   │       ├── PermissionsController .cs
│   │       ├── CountryController.cs
│   │       ├── ABACController.cs
│   │       ├── AttributeDefinitionsController.cs
│   │       ├── AttributeGroupsController.cs
│   │       ├── ConditionOperatorsController.cs
│   │       ├── PolicyRulesController.cs
│   │       ├── RolePolicyAssignmentController.cs
│   │       ├── RoleManagementController.cs
│   │       ├── PermissionManagementController.cs
│   │       └── RolePermissionController.cs
│   ├── WFT.Infra.Test/
│   │   ├── Pagination/
│   │   │   ├── PaginationResponseTests.cs
│   │   │   └── JsonApiFormatTests.cs
│   │   ├── RBAC/
│   │   │   └── RbacVerificationTests.cs
│   │   └── Helpers/TestDataSeeder.cs
│   └── WFT.Test/
│       └── Legacy/possibly unrelated MSTest project referencing missing `WFT.Bussines`.
```

Critical files:
- `src/WFT.Infra.WebApi/Program.cs`: app bootstrap, config providers, controller JSON options, JWT auth, Swagger, response middleware, startup seeding/migration.
- `src/WFT.Infra.Bootstrapper/DependencyInjection.cs`: composes Application + Infrastructure dependencies.
- `src/WFT.Infra.Application/DependencyInjection.cs`: registers most application services, AutoMapper, MediatR, password hasher, ABAC/RBAC services.
- `src/WFT.Infra.Infrastructure/DependencyInjection.cs`: registers EF Core `ApplicationDbContext`, repositories, DB initializer, infrastructure JWT generator.
- `src/WFT.Infra.Infrastructure/Data/ApplicationDbContext.cs`: EF DbSets and timestamp handling.
- `src/WFT.Infra.Infrastructure/Configurations/*.cs`: EF table names, indexes, relationships, constraints.
- `src/WFT.Infra.Infrastructure/Repositories/UserRepository.cs`: optimized RBAC permission query.
- `src/WFT.Infra.Application/Services/UserManagment/LoginService.cs`: enhanced login response with roles/permissions and refresh token.
- `src/WFT.Infra.Application/Services/UserManagment/ABACService.cs`: RBAC+ABAC evaluation.
- `src/WFT.Infra.WebApi/CustomConfig/WFTJsonResult.cs` and `WFTResponseMiddleware.cs`: unified response formatting.

## 3. Core Logic & Architecture

Architecture style:
- Mostly **Clean/Layered Architecture**:
  - **Core**: entity model only.
  - **Application.Contracts**: DTOs, interfaces, request/response models, repository contracts.
  - **Application**: business/application services, mapping, seeding, authorization orchestration.
  - **Infrastructure**: EF Core, migrations, repositories, database initialization, infrastructure JWT generator.
  - **Bootstrapper**: dependency composition.
  - **WebApi**: controllers, middleware, startup configuration.
- Uses **Repository pattern** through `IRepository<TEntity>` and `Repository<TEntity>`.
- Uses **DTO mapping** with AutoMapper profiles.
- Uses **service-per-domain** pattern: `UserService`, `RoleService`, `PermissionService`, `AttributeService`, `PolicyRuleService`, `ABACService`.
- Uses **JWT Bearer authentication** and controller-level `[Authorize]` through `BaseController`.

Dependency direction:
- `WebApi` references `Bootstrapper`.
- `Bootstrapper` references `Application`, `Infrastructure`, and contracts.
- `Application` references `Application.Contracts` and `Core`.
- `Infrastructure` references `Core` and `Application.Contracts`.
- `Core` has no project references.

Domain model:
- `BaseEntity` provides `Id`, audit timestamps, `CreatedUserId`, `UpdatedUserId`, and `RowVersion`.
- RBAC:
  - `User` ↔ `Role` through `UserRole`.
  - `Role` ↔ `Permission` through `RolePermission`.
  - `UserPassword` is one-to-one with `User`.
- ABAC:
  - `AttributeGroup` groups `AttributeDefinition`.
  - `AttributeDefinition` describes named attributes with `DataType` and `Source` (`User`, `Resource`, `Environment`, `Action`).
  - `AttributeValue` stores user/resource attribute values.
  - `ConditionOperator` defines operators such as `Equals`, `Contains`, `GreaterThan`, `Between`, `In`.
  - `PolicyRule` has `PolicyCondition` children and `Effect` (`Allow`/`Deny` intent; EF constraint currently expects uppercase values).
  - `RolePolicyRule` assigns ABAC policies to roles.

EF Core:
- `ApplicationDbContext` declares DbSets for `Countries`, `Users`, `Roles`, `Permissions`, `RefreshTokens`, `UserPasswords`, RBAC junctions, and ABAC tables.
- `OnModelCreating` applies all configurations from the Infrastructure assembly.
- `SaveChangesAsync` automatically sets `CreatedAt` for added `BaseEntity` records and `UpdatedAt` for modified records.
- `DatabaseInitializer.Initialize()` calls `Database.Migrate()` during application startup.

Repository behavior:
- `Repository<TEntity>` provides basic CRUD, count, exists, expression find, `Table`, `TableNoTracking`, and `GetPagedAsync`.
- `UpdateAsync` handles already-tracked entities before finding/updating existing entities.
- There is a suspicious legacy method `UpdateAsynccccc`; avoid using it and consider removing/refactoring only in a dedicated cleanup task.
- `UserRepository.HasPermissionAsync` normalizes the permission name with trim/lowercase and checks active permissions through `Users -> UserRoles -> Role -> RolePermissions -> Permission`.

Authentication/authorization:
- `Program.cs` configures JWT Bearer validation using `JwtSettings` (`Secret`, `Issuer`, `Audience`, `ExpiryInMinutes`).
- `WorkContext` reads current `UserId` from `ClaimTypes.NameIdentifier`, roles from `ClaimTypes.Role`, and permissions from `"permission"` claims.
- `AuthorizationService.HasPermissionAsync` first checks RBAC via `UserService.HasPermissionAsync`; if a resource is supplied, it then evaluates ABAC.
- `ABACService.EvaluateAccessAsync` also checks basic RBAC before evaluating assigned policies.

API response style:
- Controllers inheriting `BaseController` are routed as `/api/app/v1/wft/[controller]` and are authorized by default.
- `WFTJsonResult` creates a flat response object: `success`, `statusCode`, `message`, `logId`, `meta`, `data`.
- `WFTResponseMiddleware` wraps ordinary responses unless the body already contains `"success"` and `"data"`.
- `BaseController.PaginatedResponse<T>` returns `ApiEnvelope<IEnumerable<T>>` with flat `data` and `meta`.
- JSON options in `Program.cs` use camelCase, indented output, and `ReferenceHandler.IgnoreCycles`.

Important implementation caveats:
- `AuthService.RegisterAsync` appears to set password before adding the user, while `UserService.RegisterByUserAsync` correctly adds the user first and then sets password. Prefer `RegisterByUserAsync` or fix `RegisterAsync` in a focused auth task.
- `ABACService.ValidatePolicyRuleAsync` calls condition evaluation with `userId = 0`; detailed evaluation passes the real user ID. This means simple policy validation may not correctly evaluate user-source attributes.
- `PolicyRuleConfiguration` check constraint uses `Effect IN ('ALLOW', 'DENY')`, but seed/docs/services compare/use `Allow` and `Deny`. This casing mismatch can cause DB insert/evaluation bugs unless normalized.
- `RolePermissionManagement` services/controllers exist, but their DI registration and AutoMapper maps are not visible in `Application.DependencyInjection`/profiles. They may not resolve at runtime until registered.
- `src/WFT.Infra.Application/Mapping/Profiles/UserProfile.cs` contains old `RPK.*` namespaces and is explicitly removed from compile in the Application csproj. Do not copy patterns from it.
- `src/WFT.Infra.WebApi/Controllers/PermissionsController .cs` has a space before `.cs`; preserve path exactly when editing unless intentionally renaming in a focused task.

## 4. Critical Data Flows & Entry Points

### Application startup
1. `Program.cs` creates builder and loads `appsettings.json`, environment-specific JSON, and environment variables.
2. Controllers are registered with camelCase JSON options.
3. `AddProjectDependencies` registers Application and Infrastructure services.
4. JWT settings are bound and JWT Bearer auth is configured.
5. Swagger is configured with Bearer auth support.
6. `WFTResponseMiddleware` is added before HTTPS/auth/controller middleware.
7. On startup, a service scope resolves `InitialDataSeeder` and calls `SeedAsync()`.
8. The same scope resolves `IDatabaseInitializer` and calls `Initialize()`, which runs EF migrations.
9. Controllers are mapped.

### Login request
Endpoint: `POST /api/app/v1/wft/Auth/login`
1. `AuthController.Login` validates username/password presence.
2. Calls `ILoginService.LoginWithRolesAndPermissionsAsync`.
3. `LoginService` loads user by `Username`.
4. `LoginService` retrieves password via `IUserPasswordService.GetPasswordForAuthAsync`.
5. Password is verified with `BCrypt.Net.BCrypt.Verify`.
6. User roles are loaded with `IUserService.GetRolesForUserAsync`.
7. Role permissions are aggregated with `IRoleService.GetPermissionsForRoleAsync(roleIds)`.
8. `TokenService.GenerateToken(TokenRequest)` delegates to `IJwtTokenGenerator.GenerateToken`.
9. A secure random refresh token is generated and saved through `IRefreshTokenService.AddAsync`.
10. `AuthController` stores the refresh token in an HttpOnly, Secure, Strict cookie named `refresh_token`.
11. Returns `WFTJsonResult.Ok` with `accessToken`, user summary, `roles`, and `permissions`.

### Refresh token request
Endpoint: `POST /api/app/v1/wft/Auth/refresh-token`
1. `AuthController.RefreshToken` reads `refresh_token` cookie.
2. Calls `ITokenService.RefreshTokenAsync`.
3. `TokenService` loads refresh token record and rejects revoked/expired tokens.
4. Loads user, roles, and permissions.
5. Generates a new access token with roles and permission names.
6. Revokes the old refresh token and stores a new one.
7. Controller replaces the secure cookie and returns the new access token.

### RBAC permission check
Typical caller: `AuthorizationService.HasPermissionAsync(userId, permissionName)`.
1. Calls `UserService.HasPermissionAsync`.
2. `UserService` delegates to `IUserRepository.HasPermissionAsync`.
3. `UserRepository` normalizes permission text.
4. EF query traverses `Users -> UserRoles -> Role.RolePermissions -> Permission`.
5. Returns true only if an active permission name matches.

### ABAC detailed access evaluation
Endpoint: `POST /api/app/v1/wft/ABAC/evaluate-detailed`
1. `ABACController` validates model state.
2. Reads current user ID from `IWorkContext.UserId`.
3. Checks if current user has `"EvaluateAccessDetailed"` permission through `IAuthorizationService.HasPermissionAsync`.
4. Calls `IABACService.EvaluateAccessDetailedAsync(request.UserId, request.Permission, request.Resource, request.Context)`.
5. `ABACService` collects user attributes and optional resource attributes.
6. Performs base RBAC check for the target user and target permission.
7. Loads applicable policies for all target user roles through `PolicyRuleService.GetPoliciesForRoleAsync`.
8. Evaluates policies by priority:
   - `Deny` policy with met conditions immediately rejects.
   - `Allow` policy with met conditions immediately allows.
   - If no policies match, default deny.
9. Returns detailed policy/condition results and reason text.

### Paginated list request
Example: `GET /api/app/v1/wft/Users/List`
1. Controller receives/binds `PagedQueryRequest` (`SearchTerm`, `Filters`, `PageNumber`, `PageSize`, optional sorting fields).
2. Service builds an expression filter, often combining search with simple exact-match filter items.
3. Repository `GetPagedAsync` counts total rows, applies `Skip`/`Take`, and returns `PagedList<TEntity>`.
4. Service maps entities to DTOs and returns `PagedList<Dto>`.
5. Controller calls `PaginatedResponse(result.Items, request.PageNumber, request.PageSize, result.TotalCount)`.
6. Response shape is `success`, flat `data`, and `meta` containing `page`, `pageSize`, `totalItems`, and `totalPages`.

## 5. Coding Guidelines & Standards

Follow these rules when modifying this codebase:

1. **Always update this file** (`GapCodeDocs/system_analysis.md`) after any code/config/test/API/migration change.
2. Preserve the layered dependency direction. Do not reference Infrastructure from Application or Core.
3. Put domain entities in `WFT.Infra.Core/Entities`; put contracts/DTOs/interfaces in `WFT.Infra.Application.Contracts`; put business logic in `WFT.Infra.Application`; put EF/repositories/migrations in `WFT.Infra.Infrastructure`; put HTTP-only concerns in `WFT.Infra.WebApi`.
4. Use async service/repository methods consistently.
5. Use `IRepository<T>` for generic CRUD and specialized repositories only when query logic is domain-specific/optimized.
6. Use AutoMapper profiles for entity/DTO conversion; do not manually map repeatedly unless necessary for custom projections.
7. Keep API responses consistent:
   - Prefer `WFTJsonResult.Ok/Fail` for explicit success/failure responses.
   - Use `BaseController.PaginatedResponse` for list endpoints with pagination.
   - Keep JSON camelCase for wire responses.
8. New secured controllers should inherit `BaseController` unless there is a deliberate route/authorization exception.
9. Public API route style in existing base controllers is `/api/app/v1/wft/[controller]`; role-permission management controllers currently use `/api/[controller]`.
10. Passwords must be hashed with BCrypt; never store or return plaintext passwords.
11. JWT settings come from `JwtSettings`; avoid hardcoding secrets outside local config.
12. Keep permission names consistent. Existing seed uses names like `user.create`, while some controller checks use names like `EvaluateAccessDetailed`; normalize or document new permission names when adding endpoints.
13. When changing EF entities, update configurations and add migrations in Infrastructure.
14. Be careful with check constraints and enum-like strings; normalize casing before insert/evaluation.
15. Avoid introducing one-letter variable names or unrelated cleanup.
16. Preserve mixed Persian/English user-facing messages where present; localize new messages consistently with surrounding code.
17. Do not rely on `UserProfile.cs` under `Mapping/Profiles` because it is excluded from compile and uses old namespaces.
18. If adding new services/controllers, register DI and add AutoMapper mappings in the same task.
19. Prefer fixing root causes over adding controller-level workarounds.
20. Before finishing a coding task, run the most targeted available build/tests if `dotnet` is available; if not, record validation as blocked in this document.

## 6. Current Project State & Progress Log

### Current backend status

Functional/mostly implemented:
- ASP.NET Core Web API project boots through `Program.cs` with JWT auth, Swagger, response middleware, startup seed, and auto-migration logic.
- Clean/layered project structure is present.
- Core RBAC entities, DbSets, EF configurations, junction tables, and optimized permission query exist.
- User, role, permission, country, password, ABAC attribute/group/operator/policy controllers and services exist.
- Enhanced login flow exists and returns access token, user summary, role names, and permission names.
- Refresh token storage/revocation flow exists.
- ABAC model exists with attribute definitions/values, condition operators, policy rules, policy conditions, and role-policy assignment.
- ABAC detailed evaluation exists with per-policy and per-condition results.
- Initial seeding creates attribute groups, condition operators, basic attribute definitions, an Admin role, several permissions, SuperAdmin user, and user attributes.
- JSON:API-like pagination models/tests exist: `PagedResponse<T>`, `Meta`, `ApiEnvelope<T>`, pagination tests, JSON format tests.
- RBAC verification tests exist using EF Core InMemory and xUnit.

Partially implemented / needs verification:
- Role-permission management API (`RoleManagementController`, `PermissionManagementController`, `RolePermissionController`) and services/DTOs exist but likely need DI registration and AutoMapper profile mappings.
- `PermissionHandler`, `PermissionRequirement`, and `HasPermissionAttribute` exist but dynamic policy registration is not fully wired in `DependencyInjection.cs`.
- `ABACService` supports user/resource/environment attributes partially; action attributes return null and environment support is limited to current time/date.
- Sorting fields in `PagedQueryRequest` exist but services generally do not apply sorting.
- Response middleware currently wraps ordinary responses and skips wrapping if body contains `"success"` and `"data"`; older docs mention more robust direct `PagedResponse<T>` detection, but current middleware code does not show that generic detection.
- Migrations exist, but build/test could not be verified in this shell because `dotnet` command is unavailable.

Known issues / risks:
- `dotnet` is not installed/available in the current bash environment, so `dotnet build WFT.Infra.sln --no-restore` failed with `dotnet: command not found` on 2026-07-04.
- `PolicyRuleConfiguration` uses SQL check constraint `Effect IN ('ALLOW', 'DENY')`, while application logic compares `"Allow"`/`"Deny"`. Fix by standardizing casing in DTO/service/config/migrations.
- `AuthService.RegisterAsync` appears to call `SetPasswordAsync` before the user is persisted and before a real `UserId` is available. Prefer/fix the `UserService.RegisterByUserAsync` pattern.
- `ABACService.ValidatePolicyRuleAsync` evaluates user attributes with `userId = 0`; simple validation may fail for user-scoped policy conditions.
- Role-permission management services are unregistered in observed `Application.DependencyInjection`; controllers may fail DI activation.
- AutoMapper mappings for RolePermissionManagement DTOs are not visible in current profiles.
- `src/WFT.Test/WFT.Test.csproj` references `..\..\WFT.Bussines\WFT.Bussines.csproj`, which is not present in this repo view and may break solution builds if included.
- `PermissionsController .cs` has an unusual filename with a space.
- Multiple old summary docs conflict on pagination history (PascalCase nested vs final camelCase flat). Treat the current source code as canonical.
- Many local files are modified/untracked before this analysis. Avoid broad formatting or unrelated edits.

### Chronological progress log of recent backend changes

Recent git history and project docs indicate this sequence:

1. Added user/password groundwork and DTO IDs.
2. Added password-setting service and user password management.
3. Added role controller and role-related APIs.
4. Added RBAC permission data and permission checking.
5. Added ABAC entities, services, controllers, documentation, and migrations.
6. Fixed RBAC junction tables and EF configurations:
   - Added `UserRoles` and `RolePermissions` DbSets.
   - Added `UserRoleConfiguration`, `RolePermissionConfiguration`, and related user/role/permission configs.
   - Added unique composite indexes for junction tables.
   - Fixed `UserRepository.HasPermissionAsync`.
7. Added xUnit RBAC verification tests with EF Core InMemory.
8. Refactored pagination/response models toward flat camelCase `success/data/meta/message/error`.
9. Added enhanced login response with roles and permissions via `LoginService`.
10. Added Role-Permission Management DTOs, services, and controllers; integration still appears incomplete.
11. Latest commit shown by git: `fb0ca6d permission checker`.
12. 2026-07-04: Created this AI-ready system analysis document at `GapCodeDocs/system_analysis.md`.

### Immediate next tasks

Highest priority:
1. Install/use a .NET 9 SDK environment and run `dotnet build WFT.Infra.sln` and `dotnet test src/WFT.Infra.Test/WFT.Infra.Test.csproj`.
2. Decide whether `src/WFT.Test` is legacy; remove from solution or fix its missing `WFT.Bussines` reference if it blocks builds.
3. Register RolePermissionManagement services in DI:
   - `WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement.IRoleService`
   - `IPermissionService`
   - `IRolePermissionService`
   - Implementations under `WFT.Infra.Application.Services.RolePermissionManagement`.
4. Add AutoMapper mappings for RolePermissionManagement DTOs/entities.
5. Fix `PolicyRule.Effect` casing mismatch across config, migrations, seed, DTOs, and `ABACService`.
6. Fix `AuthService.RegisterAsync` persistence/password ordering or route registration to the safer registration flow.
7. Fix `ABACService.ValidatePolicyRuleAsync` so it receives/evaluates with the real user ID when validating user-source conditions.

Recommended follow-up:
1. Normalize permission naming and seed all permissions required by controller-level checks (`EvaluateAccess`, `ViewPolicies`, etc.).
2. Add integration tests for login, refresh-token, RBAC authorization, and ABAC detailed evaluation.
3. Implement sorting in paged services using `PagedQueryRequest.SortBy` and `SortDirection`.
4. Review `WFTResponseMiddleware` for robust wrapping behavior, status codes, error serialization, and non-JSON responses.
5. Clean up excluded/legacy files only in focused tasks (`UserProfile.cs`, `UpdateAsynccccc`, stale docs, legacy test project).

### Validation status

- Source inspection completed on 2026-07-04.
- `dotnet build WFT.Infra.sln --no-restore` attempted on 2026-07-04 and could not run because `dotnet` was not found in this shell.
- No automated tests were run in this session for the same reason.

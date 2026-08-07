
---

### `Backend_Summary.md`
```md
# Backend Summary

## 1. Platform Overview

This backend is a .NET 10 ASP.NET Core Web API built using Clean Architecture principles.

The system is designed to provide:

- Authentication and authorization
- User, role, and permission management
- RBAC and ABAC support
- API contracts for a React frontend
- SQL Server persistence
- JWT-based security
- Swagger/OpenAPI documentation
- Migration and seeding support

The project entry point is:

- `src/WFT.Infra.WebApi`

---

## 2. Core Technologies

The backend uses the following technologies and packages:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- JWT Bearer authentication
- Swagger / OpenAPI
- AutoMapper
- MediatR
- BCrypt

The architecture is controller-based and follows layered separation of concerns.

---

## 3. Solution Architecture
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

---

## 4. Authorization Change - SuperAdmin Bypass

### 4.1 Change Type

- Security / authorization behavior
- Backend application service
- Automated RBAC/ABAC verification

### 4.2 Reason for Change

The seeded `SuperAdmin` role represents the system-level administrator. Requiring this role to also receive every individual permission assignment caused valid administration operations to be denied when explicit RBAC permissions or ABAC policies were not assigned.

### 4.3 Affected Files and Components

- `src/WFT.Infra.Application/Services/AuthorizationService.cs`
  - `HasPermissionAsync`
  - `EvaluateAccessDetailedAsync`
  - `IsSuperAdminAsync`
- `src/WFT.Infra.Test/RBAC/RbacVerificationTests.cs`
  - SuperAdmin bypass verification
  - Normal-role authorization regression verification

### 4.4 Behavior Before and After

**Before:**

- Every user, including `SuperAdmin`, passed through the explicit permission check.
- Resource-based requests could also pass through ABAC evaluation.
- A `SuperAdmin` user without assigned permissions could be denied.

**After:**

- `AuthorizationService` first loads the user's roles.
- A case-insensitive `SuperAdmin` role match grants access immediately.
- `HasPermissionAsync` bypasses both RBAC permission lookup and ABAC evaluation for `SuperAdmin`.
- `EvaluateAccessDetailedAsync` returns an allowed result with an explicit SuperAdmin evaluation reason.
- Users without the `SuperAdmin` role continue through the existing RBAC and ABAC flow without behavior changes.

### 4.5 Verification

Automated coverage was added to verify:

- `SuperAdmin` receives access without an assigned permission.
- RBAC and ABAC checks are not invoked after the SuperAdmin bypass succeeds.
- Detailed evaluation reports that access was granted because of the `SuperAdmin` role.
- A normal role with no required permission is still denied through the standard permission path.

Recommended targeted verification command:

```bash
dotnet test src/WFT.Infra.Test/WFT.Infra.Test.csproj --filter "FullyQualifiedName~RbacVerificationTests"
```

### 4.6 Security Notes

- The bypass is enforced on the backend, which remains the authorization source of truth.
- The role-name comparison is case-insensitive but requires the exact logical role name `SuperAdmin`.
- Assignment of the `SuperAdmin` role must be tightly controlled, audited, and limited to trusted administrators.
- This bypass intentionally skips permission and policy evaluation; it must not be generalized to other roles without a separate security review.
- Frontend permission visibility is not a substitute for this server-side enforcement.

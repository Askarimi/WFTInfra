
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

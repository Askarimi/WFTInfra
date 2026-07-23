# Role-Permission Management API - Setup & Integration Guide

**Date:** October 24, 2025  
**Status:** Ready to Compile and Deploy  
**ASP.NET Version:** .NET 9

---

## 📋 Files Created

### DTOs (7 files):
- `CreateRoleDto` - Input for creating roles
- `UpdateRoleDto` - Input for updating roles
- `RoleDetailDto` - Output for role data
- `CreatePermissionDto` - Input for creating permissions
- `UpdatePermissionDto` - Input for updating permissions
- `PermissionDetailDto` - Output for permission data
- `AssignPermissionsDto` - Input for assigning permissions to roles
- `RemovePermissionDto` - Input for removing permissions from roles
- `RoleWithPermissionsDto` - Output for roles with nested permissions

**Location:** `src/WFT.Infra.Application.Contracts/DTOs/RolePermissionManagement/`

### Service Interfaces (3 files):
- `IRoleService` - Role CRUD operations
- `IPermissionService` - Permission CRUD operations
- `IRolePermissionService` - Role-Permission linking

**Location:** `src/WFT.Infra.Application.Contracts/Interfaces/RolePermissionManagement/`

### Service Implementations (3 files):
- `RoleService` - Implements IRoleService
- `PermissionService` - Implements IPermissionService
- `RolePermissionService` - Implements IRolePermissionService

**Location:** `src/WFT.Infra.Application/Services/RolePermissionManagement/`

### Controllers (3 files):
- `RoleManagementController` - CRUD endpoints for roles
- `PermissionManagementController` - CRUD endpoints for permissions
- `RolePermissionController` - Linking endpoints

**Location:** `src/WFT.Infra.WebApi/Controllers/`

---

## 🔧 Dependency Injection Setup

Add to your `Program.cs` or `DependencyInjection.cs` (in the Bootstrapper project):

```csharp
// In Program.cs or DependencyInjection.cs
public static IServiceCollection AddRolePermissionManagement(this IServiceCollection services)
{
    // Register Services
    services.AddScoped<IRoleService, RoleService>();
    services.AddScoped<IPermissionService, PermissionService>();
    services.AddScoped<IRolePermissionService, RolePermissionService>();
    
    return services;
}

// Usage in Program.cs:
builder.Services.AddRolePermissionManagement();
```

---

## 📊 API Endpoints Reference

### Role Management (`/api/rolemanagement`)

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/rolemanagement` | List all roles | - | `WFTJsonResult` with `RoleDetailDto[]` |
| GET | `/api/rolemanagement/{id}` | Get role by ID | - | `WFTJsonResult` with `RoleDetailDto` |
| POST | `/api/rolemanagement` | Create new role | `CreateRoleDto` | `WFTJsonResult` with `RoleDetailDto` |
| PUT | `/api/rolemanagement/{id}` | Update role | `UpdateRoleDto` | `WFTJsonResult` with `RoleDetailDto` |
| DELETE | `/api/rolemanagement/{id}` | Delete role | - | `WFTJsonResult` |

### Permission Management (`/api/permissionmanagement`)

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/permissionmanagement` | List all permissions | - | `WFTJsonResult` with `PermissionDetailDto[]` |
| GET | `/api/permissionmanagement/{id}` | Get permission by ID | - | `WFTJsonResult` with `PermissionDetailDto` |
| POST | `/api/permissionmanagement` | Create new permission | `CreatePermissionDto` | `WFTJsonResult` with `PermissionDetailDto` |
| PUT | `/api/permissionmanagement/{id}` | Update permission | `UpdatePermissionDto` | `WFTJsonResult` with `PermissionDetailDto` |
| DELETE | `/api/permissionmanagement/{id}` | Delete permission | - | `WFTJsonResult` |

### Role-Permission Linking (`/api/rolepermission`)

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/rolepermission/role/{roleId}` | Get permissions for role | - | `WFTJsonResult` with `PermissionDetailDto[]` |
| POST | `/api/rolepermission/assign` | Assign permissions to role | `AssignPermissionsDto` | `WFTJsonResult` |
| POST | `/api/rolepermission/remove` | Remove permission from role | `RemovePermissionDto` | `WFTJsonResult` |
| GET | `/api/rolepermission/with-permissions` | Get all roles with permissions | - | `WFTJsonResult` with `RoleWithPermissionsDto[]` |

---

## 💾 Data Models

### Role Entity
```csharp
public class Role : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}
```

### Permission Entity
```csharp
public class Permission : BaseEntity
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<RolePermission> RolePermissions { get; set; }
}
```

### RolePermission Entity (Junction)
```csharp
public class RolePermission : BaseEntity
{
    public long RoleId { get; set; }
    public Role Role { get; set; }
    
    public long PermissionId { get; set; }
    public Permission Permission { get; set; }
}
```

---

## 📝 Request/Response Examples

### Create Role
```bash
POST /api/rolemanagement
Content-Type: application/json

{
  "name": "Admin",
  "description": "Administrator role with full access",
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Role created successfully",
  "data": {
    "id": 1,
    "name": "Admin",
    "description": "Administrator role with full access",
    "isActive": true,
    "createdAt": "2025-10-24T10:30:00Z",
    "updatedAt": null
  },
  "meta": null
}
```

### Assign Permissions to Role
```bash
POST /api/rolepermission/assign
Content-Type: application/json

{
  "roleId": 1,
  "permissionIds": [1, 2, 3, 4, 5]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Permissions assigned to role successfully",
  "data": null,
  "meta": null
}
```

### Get Role with Permissions
```bash
GET /api/rolepermission/with-permissions
```

**Response (200 OK):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Roles with permissions retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "Admin",
      "description": "Administrator",
      "isActive": true,
      "permissions": [
        {
          "id": 1,
          "name": "CreateUser",
          "displayName": "ایجاد کاربر",
          "isActive": true,
          "createdAt": "2025-10-24T10:00:00Z",
          "updatedAt": null
        },
        {
          "id": 2,
          "name": "EditUser",
          "displayName": "ویرایش کاربر",
          "isActive": true,
          "createdAt": "2025-10-24T10:00:00Z",
          "updatedAt": null
        }
      ]
    }
  ],
  "meta": null
}
```

---

## 🗺️ Architecture Overview

```
Controllers (API Layer)
├── RoleManagementController
├── PermissionManagementController
└── RolePermissionController
    ↓
Services (Business Logic Layer)
├── RoleService (IRoleService)
├── PermissionService (IPermissionService)
└── RolePermissionService (IRolePermissionService)
    ↓
Repositories (Data Access Layer)
├── IRepository<Role>
├── IRepository<Permission>
└── IRepository<RolePermission>
    ↓
Database (Entities)
├── Role
├── Permission
└── RolePermission (Junction)
```

---

## ✅ Validation Rules

### Role Creation
- ✓ Name is required and cannot be empty
- ✓ Description is optional
- ✓ IsActive defaults to true

### Permission Creation
- ✓ Name is required and cannot be empty
- ✓ DisplayName is required and cannot be empty
- ✓ IsActive defaults to true

### Role-Permission Assignment
- ✓ RoleId must be > 0
- ✓ At least one PermissionId required
- ✓ Both role and all permissions must exist
- ✓ Duplicate assignments prevented automatically

---

## 🔐 Security & Authorization

All endpoints require authentication (`[Authorize]` attribute):
- JWT Bearer token in Authorization header required
- No specific role/permission checks on these endpoints (can be added)
- CORS/HTTPS should be enforced in production

---

## 🧪 Integration Checklist

- [ ] Add services to DI container in Program.cs
- [ ] Verify AutoMapper profiles for DTOs exist
- [ ] Ensure database migrations run (entities should already exist)
- [ ] Test endpoints with Postman/Swagger
- [ ] Add logging (optional but recommended)
- [ ] Add unit tests (optional)
- [ ] Document in API docs/Swagger

---

## 🛠️ AutoMapper Configuration

Add to your AutoMapper profile if not already present:

```csharp
// In your MappingProfile.cs or BaseProfile.cs
public class RolePermissionMappingProfile : Profile
{
    public RolePermissionMappingProfile()
    {
        // Role mappings
        CreateMap<Role, RoleDetailDto>().ReverseMap();
        CreateMap<CreateRoleDto, Role>();
        CreateMap<UpdateRoleDto, Role>();
        
        // Permission mappings
        CreateMap<Permission, PermissionDetailDto>().ReverseMap();
        CreateMap<CreatePermissionDto, Permission>();
        CreateMap<UpdatePermissionDto, Permission>();
        
        // RoleWithPermissionsDto
        CreateMap<Role, RoleWithPermissionsDto>();
    }
}
```

---

## 📈 Next Steps

1. **Register services in DI container** (Program.cs)
2. **Configure AutoMapper profiles** (if not auto-wired)
3. **Run database migrations** (if needed)
4. **Test endpoints** with Postman/Swagger
5. **Add permission checks** to controllers if needed
6. **Enable Swagger/OpenAPI** for documentation

---

## 🚀 Quick Start Example

```csharp
// 1. Create a role
POST http://localhost:5000/api/rolemanagement
{
  "name": "Editor",
  "description": "Content editor",
  "isActive": true
}
// Returns: { id: 2, name: "Editor", ... }

// 2. Get all permissions
GET http://localhost:5000/api/permissionmanagement
// Returns: [ { id: 1, name: "CreatePost", ... }, ... ]

// 3. Assign permissions to role
POST http://localhost:5000/api/rolepermission/assign
{
  "roleId": 2,
  "permissionIds": [1, 2, 3]
}
// Returns: success message

// 4. View role with permissions
GET http://localhost:5000/api/rolepermission/with-permissions
// Returns: [ { id: 2, name: "Editor", permissions: [...] }, ... ]
```

---

## 📞 Troubleshooting

| Issue | Solution |
|-------|----------|
| 401 Unauthorized | Add valid JWT token to Authorization header |
| 404 Not Found | Verify ID exists in database |
| 400 Bad Request | Check request payload matches DTO structure |
| 500 Internal Error | Check logs, verify DI registration |
| AutoMapper error | Ensure profiles are registered in Program.cs |

---

## 📚 File Structure

```
src/
├── WFT.Infra.Application.Contracts/
│   ├── DTOs/RolePermissionManagement/
│   │   ├── CreateRoleDto.cs
│   │   ├── UpdateRoleDto.cs
│   │   ├── RoleDetailDto.cs
│   │   ├── CreatePermissionDto.cs
│   │   ├── UpdatePermissionDto.cs
│   │   ├── PermissionDetailDto.cs
│   │   ├── AssignPermissionsDto.cs
│   │   ├── RemovePermissionDto.cs
│   │   └── RoleWithPermissionsDto.cs
│   └── Interfaces/RolePermissionManagement/
│       ├── IRoleService.cs
│       ├── IPermissionService.cs
│       └── IRolePermissionService.cs
├── WFT.Infra.Application/
│   └── Services/RolePermissionManagement/
│       ├── RoleService.cs
│       ├── PermissionService.cs
│       └── RolePermissionService.cs
└── WFT.Infra.WebApi/
    └── Controllers/
        ├── RoleManagementController.cs
        ├── PermissionManagementController.cs
        └── RolePermissionController.cs
```

---

**Status:** ✅ Ready for Deployment  
**Last Updated:** October 24, 2025  
**Maintainer:** Development Team


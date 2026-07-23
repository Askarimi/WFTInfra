# RBAC & ABAC Quick Reference Guide

## File Locations Quick Index

### Core Entities (Domain Model)
- `src/WFT.Infra.Core/Entities/UserManagment/User.cs` — User entity
- `src/WFT.Infra.Core/Entities/UserManagment/Role.cs` — Role entity
- `src/WFT.Infra.Core/Entities/UserManagment/Permission.cs` — Permission entity
- `src/WFT.Infra.Core/Entities/UserManagment/UserRole.cs` — User-Role junction
- `src/WFT.Infra.Core/Entities/UserManagment/RolePermission.cs` — Role-Permission junction
- `src/WFT.Infra.Core/Entities/UserManagment/PolicyRule.cs` — ABAC policy
- `src/WFT.Infra.Core/Entities/UserManagment/PolicyCondition.cs` — ABAC condition
- `src/WFT.Infra.Core/Entities/UserManagment/AttributeDefinition.cs` — ABAC attribute
- `src/WFT.Infra.Core/Entities/UserManagment/RolePolicyRule.cs` — Role-Policy junction

### DTOs (Data Transfer Objects)
- `src/WFT.Infra.Application.Contracts/DTOs/UserManagment/UserDto.cs`
- `src/WFT.Infra.Application.Contracts/DTOs/UserManagment/RoleDto.cs`
- `src/WFT.Infra.Application.Contracts/DTOs/UserManagment/PermissionDto.cs`
- `src/WFT.Infra.Application.Contracts/DTOs/UserManagment/ABACEvaluationResponseDto.cs`
- `src/WFT.Infra.Application.Contracts/DTOs/UserManagment/LoginResultDto.cs`

### Repositories
- `src/WFT.Infra.Infrastructure/Repositories/UserRepository.cs` — Custom user queries
- `src/WFT.Infra.Infrastructure/Repositories/Repository.cs` — Generic CRUD repository

### Services (Business Logic)
- `src/WFT.Infra.Application/Services/AuthService.cs` — Authentication
- `src/WFT.Infra.Application/Services/AuthorizationService.cs` — Authorization orchestration
- `src/WFT.Infra.Application/Services/UserManagment/UserService.cs` — User management
- `src/WFT.Infra.Application/Services/UserManagment/RoleService.cs` — Role management
- `src/WFT.Infra.Application/Services/UserManagment/ABACService.cs` — ABAC evaluation
- `src/WFT.Infra.Application/Services/TokenService.cs` — JWT generation

### Interfaces (Contracts)
- `src/WFT.Infra.Application.Contracts/Interfaces/UserManagment/IUserService.cs`
- `src/WFT.Infra.Application.Contracts/Interfaces/UserManagment/IRoleService.cs`
- `src/WFT.Infra.Application.Contracts/Interfaces/UserManagment/IPermissionService.cs`
- `src/WFT.Infra.Application.Contracts/Interfaces/UserManagment/IABACService.cs`
- `src/WFT.Infra.Application.Contracts/Interfaces/IAuthorizationService.cs`

### Controllers (API Endpoints)
- `src/WFT.Infra.WebApi/Controllers/AuthController.cs` — Login/Register
- `src/WFT.Infra.WebApi/Controllers/UsersController.cs` — User CRUD
- `src/WFT.Infra.WebApi/Controllers/RolesController.cs` — Role CRUD
- `src/WFT.Infra.WebApi/Controllers/PermissionsController .cs` — Permission CRUD
- `src/WFT.Infra.WebApi/Controllers/RolePolicyAssignmentController.cs` — Policy assignment
- `src/WFT.Infra.WebApi/Controllers/PolicyRulesController.cs` — Policy CRUD
- `src/WFT.Infra.WebApi/Controllers/ABACController.cs` — ABAC evaluation

### Database & Configuration
- `src/WFT.Infra.Infrastructure/Data/ApplicationDbContext.cs` — EF Core context
- `src/WFT.Infra.Infrastructure/Configurations/RoleConfiguration.cs` — Role table config
- `src/WFT.Infra.Infrastructure/Configurations/RolePermissionConfiguration.cs` — Junction config
- `src/WFT.Infra.Infrastructure/Configurations/RolePolicyRuleConfiguration.cs` — Policy config

---

## Common Tasks

### Task 1: Check if User Has Permission

**Backend Code**:
```csharp
// Inject IAuthorizationService
private readonly IAuthorizationService _authorizationService;

// Check permission
var hasPermission = await _authorizationService.HasPermissionAsync(
    userId: 5,
    permissionName: "EditUser"
);

if (!hasPermission)
    return Forbid("User does not have permission to edit users.");
```

**Database Query**:
```sql
SELECT EXISTS (
    SELECT 1 FROM Users u
    JOIN UserRoles ur ON u.Id = ur.UserId
    JOIN Roles r ON ur.RoleId = r.Id
    JOIN RolePermissions rp ON r.Id = rp.RoleId
    JOIN Permissions p ON rp.PermissionId = p.Id
    WHERE u.Id = 5 AND p.IsActive = 1 AND LOWER(p.Name) = 'edituser'
)
```

### Task 2: Get All Permissions for User

**Backend Code**:
```csharp
// Method 1: Get from UserRepository
var userDto = await _userRepository.GetByIdWithRolesAndPermissionsAsync(userId: 5);
var permissions = userDto.Permissions; // List<string>

// Method 2: Get from UserService + RoleService
var roles = await _userService.GetRolesForUserAsync(userId: 5);
var permissions = await _roleService.GetPermissionsForRoleAsync(
    roleIds: roles.Select(r => r.Id).ToList()
);
```

**Result**:
```csharp
// permissions = ["EditUser", "ViewReports", "DeleteRole", ...]
```

### Task 3: Assign Role to User

**Backend Code**:
```csharp
// Using IUserService
await _userService.AddRoleToUserAsync(
    userId: 5,
    roleIds: new List<long> { 2, 3 } // Manager, Accountant roles
);
```

**What it does**:
- Creates UserRole entries: (UserId=5, RoleId=2), (UserId=5, RoleId=3)
- User "5" now has all permissions from roles 2 and 3

### Task 4: Assign Permissions to Role

**Backend Code**:
```csharp
// Using IRoleService
await _roleService.AddPermissionsToRoleAsync(
    roleId: 2,
    permissionIds: new List<long> { 10, 11, 15 } // EditUser, ViewReports, CreateReport
);
```

**What it does**:
- Creates RolePermission entries: (RoleId=2, PermissionId=10), etc.
- All users with role 2 now have these permissions

### Task 5: Remove Permissions from Role

**Backend Code**:
```csharp
await _roleService.RemovePermissionsFromRoleAsync(
    roleId: 2,
    permissionIds: new List<long> { 10 }
);
```

**What it does**:
- Deletes RolePermission (RoleId=2, PermissionId=10)
- Users with role 2 lose "EditUser" permission (unless they have it from another role)

### Task 6: Evaluate ABAC Policy

**Backend Code**:
```csharp
// Simple evaluation
var hasAccess = await _abacService.EvaluateAccessAsync(
    userId: 5,
    permission: "EditDocument",
    resource: documentObject
);

if (!hasAccess)
    return Forbid("User cannot access this document based on ABAC policies.");

// Detailed evaluation (for debugging)
var result = await _abacService.EvaluateAccessDetailedAsync(
    userId: 5,
    permission: "EditDocument",
    resource: documentObject
);

// result.HasAccess: true/false
// result.PolicyResults: List of each policy evaluated
// result.EvaluationReason: Persian explanation
```

### Task 7: Create ABAC Policy Rule

**Backend Code**:
```csharp
var policyDto = new PolicyRuleDto
{
    Name = "DepartmentDocumentAccess",
    DisplayName = "دسترسی به اسناد دپارتمان",
    Priority = 1,
    Effect = "Allow",
    IsActive = true,
    PolicyConditions = new List<PolicyConditionDto>
    {
        new PolicyConditionDto
        {
            AttributeDefinitionId = 1,
            ConditionOperatorId = 1, // Equals
            Value = "Finance",
            LogicalOperator = "AND",
            Order = 1
        },
        new PolicyConditionDto
        {
            AttributeDefinitionId = 2,
            ConditionOperatorId = 3, // GreaterThan
            Value = "Manager",
            LogicalOperator = "AND",
            Order = 2
        }
    }
};

var createdPolicy = await _policyRuleService.AddAsync(policyDto);
```

### Task 8: Assign Policy to Role

**Backend Code**:
```csharp
await _policyRuleService.AddPolicyToRoleAsync(
    roleId: 2,
    policyRuleId: 1
);
```

**What it does**:
- Creates RolePolicyRule (RoleId=2, PolicyRuleId=1)
- Users with role 2 now subject to policy 1's ABAC conditions

---

## API Call Examples

### Login & Get Permissions

```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "ali",
  "password": "SecurePass123!"
}

Response 200:
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "base64==",
    "user": {
      "id": 5,
      "username": "ali",
      "roles": ["Manager", "Accountant"],
      "permissions": ["EditUser", "ViewReports", "CreateReport"]
    }
  },
  "isSuccess": true
}
```

### Create Role

```http
POST /api/roles/Add
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "Viewer",
  "displayName": "بیننده",
  "description": "Read-only access",
  "isActive": true
}

Response 201: RoleDto
```

### Create Permission

```http
POST /api/permissions/Add
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "EditUser",
  "displayName": "ویرایش کاربر",
  "isActive": true
}

Response 201: PermissionDto
```

### Assign Permission to Role (MISSING - NOT IMPLEMENTED)

```http
POST /api/rolepermission/assign/2/10
Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "message": "Permission assigned to role"
}
```

### Assign Role to User (MISSING - NOT IMPLEMENTED)

```http
POST /api/userrole/assign/5/2
Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "message": "Role assigned to user"
}
```

### Create ABAC Policy

```http
POST /api/policyrules/Add
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "DepartmentAccess",
  "displayName": "دسترسی دپارتمان",
  "priority": 1,
  "effect": "Allow",
  "isActive": true,
  "policyConditions": [
    {
      "attributeDefinitionId": 1,
      "conditionOperatorId": 1,
      "value": "Finance",
      "logicalOperator": "AND",
      "order": 1
    }
  ]
}

Response 201: PolicyRuleDto
```

### Assign Policy to Role

```http
POST /api/rolepolicyassignment/assign/2/1
Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "message": "Policy assigned to role"
}
```

### Evaluate ABAC Policy (Debug)

```http
POST /api/abac/evaluate
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "userId": 5,
  "permission": "EditDocument",
  "resource": { "id": 100, "status": "Approved" }
}

Response 200: ABACEvaluationResponseDto
{
  "hasAccess": true,
  "userAttributes": [...],
  "resourceAttributes": [...],
  "policyResults": [...],
  "evaluationReason": "دسترسی توسط قانون مجاز شد"
}
```

---

## Permission Hierarchy Example

```
User: Ali (ID=5)
  │
  ├─ Role: Manager (ID=2)
  │  └─ Permissions:
  │     ├─ EditUser
  │     ├─ ViewReports
  │     ├─ CreateReport
  │     └─ ABAC Policy: ManagerPolicy (ID=1)
  │        └─ Conditions:
  │           ├─ Department = Finance
  │           └─ UserLevel >= Manager
  │
  └─ Role: Accountant (ID=3)
     └─ Permissions:
        ├─ ViewReports
        ├─ ApproveExpense
        └─ GenerateLedger

Result: Ali's Permissions = [EditUser, ViewReports, CreateReport, ApproveExpense, GenerateLedger]
```

---

## Database Schema Relationships

### RBAC Relationship
```
User (1) ──→ UserRole (Many) ──→ Role (1) ──→ RolePermission (Many) ──→ Permission (1)
```

**Example Query**:
```sql
-- Get all permissions for User with ID=5
SELECT DISTINCT p.Id, p.Name, p.DisplayName
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
JOIN RolePermissions rp ON r.Id = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.Id
WHERE u.Id = 5 AND p.IsActive = 1
```

### ABAC Relationship
```
Role (1) ──→ RolePolicyRule (Many) ──→ PolicyRule (1) 
                                         │
                                         └─→ PolicyCondition (Many) 
                                             │
                                             └─→ AttributeDefinition (1) 
                                                 │
                                                 └─→ AttributeValue (Many)
```

**Example Query**:
```sql
-- Get all ABAC policies for a role
SELECT pr.Id, pr.Name, pr.Effect, pr.Priority
FROM Roles r
JOIN RolePolicyRules rpr ON r.Id = rpr.RoleId
JOIN PolicyRules pr ON rpr.PolicyRuleId = pr.Id
WHERE r.Id = 2 AND pr.IsActive = 1
ORDER BY pr.Priority
```

---

## Debugging RBAC Issues

### Issue: User doesn't have permission they should have

**Debug Steps**:
1. Check user has correct roles:
   ```csharp
   var roles = await _userService.GetRolesForUserAsync(userId: 5);
   // Verify roles are active and expected
   ```

2. Check role has correct permissions:
   ```csharp
   var perms = await _roleService.GetPermissionsForRoleAsync(roleId: 2);
   // Verify permissions are active and expected
   ```

3. Check permission name case sensitivity:
   ```csharp
   // Permission matching is case-INSENSITIVE and trims whitespace
   // "EditUser" == "edituser" == "  EditUser  "
   ```

4. Check IsActive flags:
   ```csharp
   // All three must be active: Role.IsActive, Permission.IsActive, User.IsActive
   ```

### Issue: ABAC Policy not working

**Debug Steps**:
1. Use detailed evaluation:
   ```csharp
   var result = await _abacService.EvaluateAccessDetailedAsync(userId, permission, resource);
   // Check result.PolicyResults for each condition evaluation
   ```

2. Verify RBAC passes first:
   ```csharp
   var hasRBAC = await _userService.HasPermissionAsync(userId, permissionName);
   // ABAC only runs if RBAC passes
   ```

3. Check attribute values:
   ```csharp
   var attrs = await _abacService.GetUserAttributesAsync(userId);
   // Verify attributes exist and have expected values
   ```

4. Check policy assignment to role:
   ```csharp
   var policies = await _policyRuleService.GetPoliciesForRoleAsync(roleId);
   // Verify policy is assigned and active
   ```

---

## Performance Tips

| Operation | Cost | Tips |
|-----------|------|------|
| HasPermissionAsync | 1 DB Query | Optimal — single JOIN query |
| GetRolesForUserAsync | 1 DB Query | Fast |
| GetPermissionsForRoleAsync | 2 DB Queries | OK for single role; batch for multiple |
| EvaluateAccessAsync | 2+ DB Queries | Use for resource-specific checks only |
| Evaluate ABAC Policies | Variable | Policies with many conditions slower |

**Optimization**:
- Cache user permission set in Redis (TTL: 5-15 minutes)
- Batch policy queries per user instead of per role
- Consider dedicated query optimizations for complex ABAC scenarios

---

## Migration Guide: Adding New Permission

### Step 1: Create Permission in DB
```csharp
var permissionDto = new PermissionDto
{
    Name = "DeleteReport",
    DisplayName = "حذف گزارش",
    IsActive = true,
    CreatedUserId = 1
};

var permission = await _permissionService.AddAsync(permissionDto);
```

### Step 2: Assign to Role(s)
```csharp
await _roleService.AddPermissionsToRoleAsync(
    roleId: 2, // Manager role
    permissionIds: new List<long> { permission.Id }
);
```

### Step 3: Add to Frontend Permission Checks
```javascript
// Frontend code (React/Vue/Angular)
if (permissions.includes("DeleteReport")) {
    // Show delete button
}
```

### Step 4: Protect Backend Endpoint
```csharp
var hasPermission = await _authorizationService.HasPermissionAsync(
    currentUserId,
    "DeleteReport"
);

if (!hasPermission)
    return Forbid();
```

---

## Common Mistakes to Avoid

❌ **Mistake 1**: Checking permission without loading from database first
```csharp
// WRONG: Using JWT permission claim directly
if (User.HasClaim("permission", "EditUser"))
{
    // Outdated! Permissions may have changed since login
}

// RIGHT: Query database
var hasPermission = await _authorizationService.HasPermissionAsync(userId, "EditUser");
```

❌ **Mistake 2**: Forgetting to check IsActive flags
```csharp
// WRONG: Just checking if record exists
var permission = await _permissionService.GetByIdAsync(id);

// RIGHT: Also check IsActive
if (!permission.IsActive)
    return BadRequest("Permission is inactive");
```

❌ **Mistake 3**: Not handling ABAC resource checks
```csharp
// WRONG: Only checking RBAC
var hasPermission = await _userService.HasPermissionAsync(userId, "ViewDocument");

// RIGHT: Include resource for ABAC
var hasPermission = await _authorizationService.HasPermissionAsync(
    userId,
    "ViewDocument",
    resource: document // ABAC evaluates this
);
```

❌ **Mistake 4**: Case-sensitive permission matching (frontend)
```javascript
// WRONG: Frontend assumes exact case match
if (permissions.includes("edituser"))  // Fails if backend sent "EditUser"

// RIGHT: Case-insensitive check
if (permissions.map(p => p.toLowerCase()).includes("edituser"))
```

---

## Testing Checklist

- [ ] Create permission via API
- [ ] Assign permission to role
- [ ] Assign role to user
- [ ] Call endpoint requiring permission — should succeed
- [ ] Remove permission from role
- [ ] Call endpoint requiring permission — should fail (403 Forbidden)
- [ ] Create ABAC policy with conditions
- [ ] Assign policy to role
- [ ] Test detailed policy evaluation endpoint
- [ ] Verify permission names are case-insensitive
- [ ] Test bulk assignment operations

---

**Last Updated**: October 24, 2025  
**Status**: Ready for Developer Reference



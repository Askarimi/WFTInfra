# Backend RBAC & Permission Management Analysis Report

## Executive Summary

The WFT.Infra backend implements a sophisticated **hybrid RBAC + ABAC (Role-Based & Attribute-Based Access Control)** system. The architecture supports:
- Traditional role-based permissions through RBAC
- Advanced attribute-based policies through ABAC with condition evaluation
- Policy-to-role assignment with priority-based effect rules (Allow/Deny)
- User → Roles → Permissions chain resolution
- Comprehensive audit capabilities through detailed evaluation responses

---

## 1. ENTITIES & DATA MODEL

### Core Authorization Entities

#### **1.1 User** (`User.cs`)
- **Id**: Primary key (inherited from BaseEntity)
- **Username**: Unique identifier
- **Email**: User's email address
- **FirstName, LastName**: Optional user display information
- **EmailConfirmed**: Boolean flag for email verification status
- **IsActive**: Boolean flag for user activation status
- **LastLoginAt**: DateTime for tracking last login
- **UserRoles**: Navigation collection to `UserRole` (one-to-many)
- **Password**: Navigation to `UserPassword` entity (one-to-one)

#### **1.2 Role** (`Role.cs`)
- **Id**: Primary key
- **Name**: Role identifier (e.g., "Admin", "Editor", "Viewer")
- **IsActive**: Boolean flag for role activation
- **Description**: Human-readable description
- **UserRoles**: Collection of `UserRole` (one-to-many)
- **RolePermissions**: Collection of `RolePermission` (one-to-many) - **Direct RBAC link**
- **RolePolicyRules**: Collection of `RolePolicyRule` (one-to-many) - **ABAC link**

#### **1.3 Permission** (`Permission.cs`)
- **Id**: Primary key
- **Name**: Permission identifier (e.g., "EditUser", "DeleteRole", "ViewReports")
- **DisplayName**: User-friendly name (supports Farsi/Persian text)
- **IsActive**: Boolean flag
- **RolePermissions**: Collection of `RolePermission` (one-to-many)

#### **1.4 UserRole** (`UserRole.cs`) - **Junction Table**
- **Id**: Primary key
- **UserId**: Foreign key to User
- **RoleId**: Foreign key to Role
- Links users to one or more roles (many-to-many relationship)

#### **1.5 RolePermission** (`RolePermission.cs`) - **Junction Table**
- **Id**: Primary key
- **RoleId**: Foreign key to Role
- **PermissionId**: Foreign key to Permission
- Links roles to permissions (many-to-many relationship)
- **Direct RBAC layer**: Permissions are assigned via roles

#### **1.6 RolePolicyRule** (`RolePolicyRule.cs`) - **ABAC Link**
- **Id**: Primary key
- **RoleId**: Foreign key to Role
- **PolicyRuleId**: Foreign key to PolicyRule
- **IsActive**: Boolean flag for policy activation
- **ValidFrom, ValidTo**: Optional date-time range for policy validity
- Links roles to policy rules with temporal constraints

#### **1.7 PolicyRule** (`PolicyRule.cs`) - **ABAC Policies**
- **Id**: Primary key
- **Name**: Policy identifier (e.g., "DepartmentDocumentAccess")
- **DisplayName**: User-friendly name
- **Description**: Policy description
- **IsActive**: Boolean flag
- **Priority**: Integer for rule execution order (1 = highest priority)
- **Effect**: "Allow" or "Deny" (policy outcome)
- **PolicyConditions**: Collection of `PolicyCondition` (one-to-many)
- **RolePolicyRules**: Collection of `RolePolicyRule` (one-to-many)

#### **1.8 PolicyCondition** (`PolicyCondition.cs`) - **ABAC Conditions**
- **Id**: Primary key
- **PolicyRuleId**: Foreign key to PolicyRule
- **AttributeDefinitionId**: Foreign key to AttributeDefinition
- **ConditionOperatorId**: Foreign key to ConditionOperator
- **Value**: The comparison value (stored as string, type-flexible)
- **LogicalOperator**: "AND" or "OR" - for combining conditions within a policy
- **Order**: Execution order (for AND/OR evaluation)
- Represents a single condition in an ABAC policy

#### **1.9 AttributeDefinition** (`AttributeDefinition.cs`) - **ABAC Attributes**
- **Id**: Primary key
- **Name**: Attribute identifier (e.g., "Department", "Location", "ResourceOwner")
- **DisplayName**: User-friendly name
- **DataType**: "String", "Number", "Boolean", "DateTime"
- **Source**: Where attribute comes from - "User", "Resource", "Environment", "Action"
- **IsRequired**: Whether this attribute must be present
- **DefaultValue**: Optional default value
- **IsActive**: Boolean flag
- **Description**: Attribute description
- **AttributeGroupId**: Foreign key to AttributeGroup (optional grouping)
- **AttributeValues**: Collection of `AttributeValue` (one-to-many)
- **PolicyConditions**: Collection of `PolicyCondition` (one-to-many)

#### **1.10 AttributeValue** (`AttributeValue.cs`)
- Stores actual attribute values for users or resources
- Links to AttributeDefinition
- Stores user's department, location, cost center, etc.

#### **1.11 ConditionOperator** (`ConditionOperator.cs`) - **ABAC Operators**
- **Id**: Primary key
- **Name**: Operator identifier (e.g., "Equals", "Contains", "GreaterThan", "NotEquals")
- **DisplayName**: User-friendly name
- **Symbol**: Operator symbol (e.g., "==", "contains", ">", "!=")
- **DataTypes**: Comma-separated/JSON array of supported data types (e.g., "String,Number")
- **IsActive**: Boolean flag
- **PolicyConditions**: Collection of `PolicyCondition` (one-to-many)

### Entity Relationship Diagram (Summary)

```
User (one-to-many) ──► UserRole ──► (many-to-one) Role
                                         │
                                         ├──► (many-to-many) Permission (via RolePermission)
                                         │
                                         └──► (many-to-many) PolicyRule (via RolePolicyRule)
                                                    │
                                                    └──► PolicyCondition ──► AttributeDefinition
                                                                    │
                                                                    └──► ConditionOperator
```

---

## 2. REPOSITORIES & SERVICES

### 2.1 Repositories

#### **IUserRepository & UserRepository** (`UserRepository.cs`)
**Key Methods:**
- `GetByIdWithRolesAndPermissionsAsync(long userId)`: Eagerly loads user with roles and their permissions
  - Query pattern: User → UserRoles → Roles → RolePermissions → Permissions
  - Returns UserDto with Roles and Permissions lists
- `HasPermissionAsync(long userId, string permissionName)`: **Core RBAC check**
  - Verifies if user has specific permission through role chain
  - Case-insensitive permission matching
  - Checks permission IsActive status

**Query Flow:**
```csharp
User → UserRole (where UserId = userId)
      → Role (where RoleId matches)
      → RolePermission
      → Permission (where Name matches)
```

#### **IRepository<T> & Repository<T>** (Generic Repository)
Base repository for all entities with standard CRUD operations:
- `AddAsync`, `UpdateAsync`, `DeleteAsync`
- `GetByIdAsync`, `GetAllAsync`, `GetPagedAsync`
- `GetByExpressionAsync`, `GetListByExpressionAsync`
- Supports filtering, pagination, and expression-based queries

### 2.2 Services

#### **UserService** (`UserService.cs`)
**Role & Permission Operations:**
- `AddRoleToUserAsync(long userId, List<long> roleIds)`: Assigns multiple roles to a user
- `GetRolesForUserAsync(long userId)`: Retrieves all roles assigned to a user (returns RoleDto list)
- `HasPermissionAsync(long userId, string permissionName)`: Delegates to UserRepository
- `AddAsync`, `UpdateAsync`, `DeleteAsync`: Standard user CRUD

#### **RoleService** (`RoleService.cs`)
**Role Management:**
- `AddAsync`, `UpdateAsync`, `DeleteAsync`: Role CRUD
- `GetByIdAsync`, `GetAllAsync`, `GetPagedListAsync`: Role retrieval with pagination
- `GetByNameAsync`: Retrieve role by name

**Permission Assignment to Roles:**
- `AddPermissionsToRoleAsync(long roleId, List<long> permissionIds)`: **Direct RBAC Assignment**
  - Creates RolePermission junction records
  - Supports bulk assignment
- `RemovePermissionsFromRoleAsync(long roleId, List<long> permissionIds)`: **Direct RBAC Removal**
  - Removes RolePermission records
  - Supports bulk removal

**Permission Retrieval:**
- `GetPermissionsForRoleAsync(long roleId)`: Get permissions for a single role
- `GetPermissionsForRoleAsync(List<long> roleIds)`: Get permissions for multiple roles
  - Used during login to build permission list for JWT token

#### **PermissionService** (`PermissionService.cs`)
**Permission Management:**
- `AddAsync`, `UpdateAsync`, `DeleteAsync`: Permission CRUD
- `GetByIdAsync`, `GetAllAsync`, `GetPagedListAsync`: Permission retrieval
- `GetActivePermissionsAsync`: Retrieve only active permissions

#### **ABACService** (`ABACService.cs`)
**ABAC Policy Evaluation:**
- `EvaluateAccessAsync(long userId, string permission, object? resource = null, object? context = null)`: **Core ABAC Check**
  - Step 1: Verify basic RBAC permission exists
  - Step 2: Retrieve applicable policies for user's roles
  - Step 3: Evaluate policies in priority order
  - Returns true if access allowed, false otherwise
  
- `EvaluateAccessDetailedAsync(...)`: **Detailed ABAC Evaluation**
  - Returns `ABACEvaluationResponseDto` with full evaluation audit trail
  - Includes policy results, condition evaluations, reasons
  - Supports debugging and audit logging

- `GetApplicablePoliciesAsync(long userId, string permission)`: 
  - Fetches policies linked to user's roles
  - Filters by permission name
  
- `GetUserAttributesAsync(long userId)`: Retrieves user's attribute values
- `ValidatePolicyRuleAsync(PolicyRuleDto policy, object? resource, object? context)`: Evaluates single policy

**Evaluation Logic:**
```
1. Check RBAC permission (User → Roles → Permissions)
2. If failed, deny access
3. Get applicable ABAC policies
4. For each policy (ordered by priority):
   - Evaluate all conditions (AND/OR logic)
   - If Effect="Deny" and conditions met → Deny
   - If Effect="Allow" and conditions met → Allow
5. Default: Deny (if no policies matched)
```

#### **AuthorizationService** (`AuthorizationService.cs`)
**High-Level Authorization:**
- `HasPermissionAsync(long userId, string permissionName, object? resource = null)`:
  - Performs RBAC check
  - Optionally performs ABAC check if resource provided
  - Used by controllers and PermissionHandler
  
- `EvaluateAccessDetailedAsync(...)`: Delegates to ABACService

#### **AuthService** (`AuthService.cs`)
**Authentication & Authorization Integration:**
- `LoginAsync(UserLoginDto dto)`: Returns LoginResultDto with:
  - AccessToken (JWT with claims)
  - RefreshToken (persisted in database)
  - UserDto with roles and permissions loaded

**During Login:**
1. Verify user credentials
2. Load user's roles: `GetRolesForUserAsync`
3. Load user's permissions: `GetPermissionsForRoleAsync` (for all roles)
4. Generate JWT token with role claims
5. Create refresh token

#### **TokenService** (`TokenService.cs`)
**JWT Token Generation:**
- Generates access tokens with claims:
  - UserId (NameIdentifier claim)
  - Username
  - Roles (Role claims)
  - Permissions (custom "permission" claims)

#### **WorkContext** (`WorkContext.cs`)
**Request Context Extraction:**
- `UserId`: Extracts UserId from JWT NameIdentifier claim
- `Username`: Extracts from Identity.Name
- `Roles`: Extracts Role claims from JWT
- `Permissions`: Extracts custom "permission" claims from JWT

---

## 3. CONTROLLERS & API ENDPOINTS

### Base Controller
```csharp
[Route("api/app/v1/wft/[controller]", Name = "api_wftInfra_[controller]", Order = 0)]
[Authorize]
```

All authorization controllers require authentication.

### 3.1 RolesController (`/api/app/v1/wft/roles`)

| Method | Endpoint | Function | Authorization Check |
|--------|----------|----------|----------------------|
| POST | `/Add` | Create role | `CreateRole` permission |
| GET | `/init/{id}` | Get role by ID | `ViewRole` permission |
| GET | `/List` | List roles (paginated) | `ViewRoleList` permission |
| PUT | `/update` | Update role | `EditRole` permission (resource-aware) |
| DELETE | `/delete/{id}` | Delete role | `DeleteRole` permission |

**Note:** RolesController handles role CRUD only. Permission-to-role assignment is handled separately.

### 3.2 PermissionsController (`/api/app/v1/wft/permissions`)

| Method | Endpoint | Function | Authorization Check |
|--------|----------|----------|----------------------|
| POST | `/Add` | Create permission | `CreatePermission` permission |
| GET | `/init/{id}` | Get permission by ID | `ViewPermission` permission |
| GET | `/List` | List permissions (paginated) | `ViewPermissionList` permission |
| PUT | `/update` | Update permission | `EditPermission` permission (resource-aware) |
| DELETE | `/delete/{id}` | Delete permission | `DeletePermission` permission |

### 3.3 RolePolicyAssignmentController (`/api/app/v1/wft/rolepolicyassignment`)
**Manages ABAC Policy-to-Role assignments:**

| Method | Endpoint | Function | Authorization Check |
|--------|----------|----------|----------------------|
| POST | `/assign/{roleId}/{policyRuleId}` | Assign policy to role | `AssignPolicyToRole` |
| DELETE | `/remove/{roleId}/{policyRuleId}` | Remove policy from role | `RemovePolicyFromRole` |
| GET | `/role/{roleId}` | Get policies for role | `ViewRolePolicyAssignment` |
| POST | `/bulkassign/{roleId}` | Bulk assign policies | `AssignPolicyToRole` |
| DELETE | `/bulkremove/{roleId}` | Bulk remove policies | `RemovePolicyFromRole` |

### 3.4 PolicyRulesController (`/api/app/v1/wft/policyrules`)
**ABAC Policy Management:**
- Create, read, update, delete policy rules
- Standard CRUD pattern with authorization checks

### 3.5 AuthController (`/api/app/v1/wft/auth`)
- `POST /register`: Register new user (AllowAnonymous)
- `POST /login`: Authenticate and return JWT + RefreshToken (AllowAnonymous)
- `POST /logout`: Revoke refresh token
- `POST /refresh-token`: Issue new access token using refresh token

### 3.6 UsersController (`/api/app/v1/wft/users`)

| Method | Endpoint | Function | Authorization Check |
|--------|----------|----------|----------------------|
| POST | `/Add` | Create user | `CreateUser` permission |
| GET | `/init/{id}` | Get user by ID | `ViewUser` permission |
| GET | `/List` | List users (paginated) | `ViewUserList` permission |
| PUT | `/update` | Update user | `EditUser` permission (resource-aware) |
| DELETE | `/delete/{id}` | Delete user | `DeleteUser` permission |

### 3.7 Other Authorization Controllers
- **AttributeDefinitionsController**: Manage ABAC attribute definitions
- **AttributeGroupsController**: Organize attributes into groups
- **ConditionOperatorsController**: Define comparison operators for conditions
- **ABACController**: ABAC-specific operations

---

## 4. PERMISSION EVALUATION MECHANISM

### 4.1 RBAC Evaluation (Basic)

**Permission Check Flow:**
```
1. HasPermissionAsync(userId, permissionName)
   ↓
2. UserRepository.HasPermissionAsync(userId, permissionName)
   ↓
3. Query: User
      → UserRole (UserId = userId)
      → Role
      → RolePermission
      → Permission (Name = permissionName AND IsActive = true)
   ↓
4. Return: bool (true if any match found)
```

**Implementation:** Case-insensitive matching, checks IsActive flag.

### 4.2 ABAC Evaluation (Advanced)

**Full Authorization Check Flow:**
```
1. AuthorizationService.HasPermissionAsync(userId, permission, resource?)
   ↓
2. ABACService.EvaluateAccessAsync()
   ├─ Step 1: Verify RBAC permission
   │  └─ If no RBAC permission → Return false
   │
   ├─ Step 2: Get applicable ABAC policies
   │  └─ Query: User → UserRoles → RoleIds → RolePolicyRules → PolicyRules
   │
   ├─ Step 3: Evaluate policies in priority order
   │  For each policy:
   │  ├─ Evaluate all conditions (AND/OR logic)
   │  ├─ If Effect="Deny" + conditions met → Return false (Deny wins)
   │  └─ If Effect="Allow" + conditions met → Return true (Allow)
   │
   └─ Step 4: Default deny (no matching policy)
      └─ Return false
```

### 4.3 Policy Condition Evaluation

**Single Condition:**
```
AttributeValue [User/Resource/Environment]
        ↓
Compare with ConditionOperator (==, contains, >, <, !=, etc.)
        ↓
Against PolicyCondition.Value
        ↓
Result: true/false
```

**Multiple Conditions:**
- Combined with LogicalOperator ("AND" or "OR")
- Evaluated in Order sequence
- AND: All must be true
- OR: At least one must be true

### 4.4 Authorization Handler Integration

**PermissionHandler** (`PermissionHandler.cs`):
- Implements `AuthorizationHandler<PermissionRequirement>`
- Triggered when controller has `[HasPermission("PermissionName")]` attribute
- Extracts UserId from JWT claims
- Calls `HasPermissionAsync` for evaluation
- Sets context.Succeed() or context.Fail()

### 4.5 Caching & Performance Considerations

**Current Implementation:**
- No explicit caching layer in core RBAC/ABAC logic
- Direct database queries per request
- JWT tokens carry role and permission claims to reduce database queries on each request
- Permissions evaluated once during token generation and carried in claims

**Opportunities for Optimization:**
- Could implement IMemoryCache for frequently-checked permissions
- Could cache policy rules with invalidation on policy changes
- Could implement distributed cache (Redis) for multi-instance deployments

---

## 5. INTEGRATION & FRONTEND CONSIDERATIONS

### 5.1 Backend-to-Frontend Integration

**Login Response (LoginResultDto):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "secure_refresh_token_value",
  "user": {
    "id": 123,
    "username": "admin",
    "email": "admin@example.com",
    "firstName": "Administrator",
    "lastName": "User",
    "isActive": true,
    "lastLoginAt": "2025-10-24T10:00:00Z",
    "roles": ["Admin", "Editor"],        // ← Role names
    "permissions": ["CreateUser", "EditUser", "DeleteUser", ...]  // ← Permission names
  }
}
```

**JWT Token Claims:**
- `sub` (Subject): UserId
- `name`: Username
- `role`: Role names (multiple claims for multiple roles)
- `permission`: Permission names (multiple claims for multiple permissions)

### 5.2 Frontend Authorization Flow

**Frontend can:**
1. Store JWT token with roles and permissions from login response
2. Check permissions locally using WorkContext-equivalent logic
3. Hide/show UI elements based on permissions
4. Make API calls that automatically include JWT in Authorization header

**Frontend should NOT:**
- Modify JWT token to add fake permissions
- Rely solely on frontend checks (backend still validates on each request)

### 5.3 Missing Components for Dynamic Role-Permission Management

#### **Critical Gap 1: No Dedicated Role-Permission Assignment Endpoint**
❌ **Missing:** Frontend endpoint to assign/remove permissions directly from roles

**Current Workaround:** RoleService has methods but NO controller endpoints:
- `AddPermissionsToRoleAsync(long roleId, List<long> permissionIds)`
- `RemovePermissionsFromRoleAsync(long roleId, List<long> permissionIds)`

**Impact:** Frontend cannot dynamically manage role-permission assignments through the API.

**Solution Needed:** Create RolePermissionAssignmentController with endpoints:
```
POST /api/app/v1/wft/rolepermissionassignment/assign/{roleId}/{permissionId}
DELETE /api/app/v1/wft/rolepermissionassignment/remove/{roleId}/{permissionId}
GET /api/app/v1/wft/rolepermissionassignment/role/{roleId}
POST /api/app/v1/wft/rolepermissionassignment/bulkassign/{roleId}
DELETE /api/app/v1/wft/rolepermissionassignment/bulkremove/{roleId}
```

#### **Critical Gap 2: User-Role Assignment Endpoints**
⚠️ **Partial:** User-Role assignment exists in service but no controller endpoints

**Current State:** UserService has:
- `AddRoleToUserAsync(long userId, List<long> roleIds)`

**Missing:** Controller endpoints to assign/remove roles from users

**Solution Needed:** Create UserRoleAssignmentController:
```
POST /api/app/v1/wft/userroleassignment/assign/{userId}/{roleId}
DELETE /api/app/v1/wft/userroleassignment/remove/{userId}/{roleId}
GET /api/app/v1/wft/userroleassignment/user/{userId}
POST /api/app/v1/wft/userroleassignment/bulkassign/{userId}
```

#### **DTOs for Assignment Operations**
The following DTOs are needed for frontend-backend communication:
```csharp
public record AssignRolePermissionRequest
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
}

public record BulkAssignRequest
{
    public long[] ItemIds { get; set; }  // Permission or Role IDs
}

public record RoleWithPermissionsDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<PermissionDto> AssignedPermissions { get; set; }
    public List<PermissionDto> AvailablePermissions { get; set; }
}

public record UserWithRolesDto
{
    public long Id { get; set; }
    public string Username { get; set; }
    public List<RoleDto> AssignedRoles { get; set; }
    public List<RoleDto> AvailableRoles { get; set; }
}
```

### 5.4 Permission Initialization

**Seed Data:** The system relies on initial data seeding (`InitialDataSeeder.cs`):
- Should populate default permissions
- Should create default roles with permission assignments
- Should create system users with roles

---

## 6. STRENGTHS

✅ **Well-Designed RBAC Foundation**
- Clear User → Roles → Permissions chain
- Proper use of junction tables for many-to-many relationships

✅ **Advanced ABAC Implementation**
- Flexible policy rule engine with conditions
- Support for Allow/Deny effects with priority ordering
- Attribute-based evaluation from multiple sources (User, Resource, Environment, Action)
- Detailed audit trail through EvaluateAccessDetailedAsync

✅ **Clean Service Layer Architecture**
- Separation of concerns (AuthService, AuthorizationService, ABACService)
- Dependency injection for flexibility
- Comprehensive interface definitions

✅ **JWT Token Integration**
- Roles and permissions embedded in JWT claims
- Reduces database queries for permission checks
- Claims automatically extracted via WorkContext

✅ **Authorization Throughout**
- All API endpoints validate permissions
- Both RBAC and ABAC checks available
- Consistent error handling with descriptive messages

✅ **Farsi/Persian Support**
- DisplayName fields support localized text
- Error messages in Persian

---

## 7. GAPS & LIMITATIONS

❌ **Missing Role-Permission Management Endpoints**
- `RoleService.AddPermissionsToRoleAsync()` exists but no controller endpoint
- Frontend cannot dynamically assign permissions to roles

❌ **Missing User-Role Assignment Endpoints**
- No dedicated controller for user-role relationships
- `UserService.AddRoleToUserAsync()` hidden from API

❌ **No Caching Layer**
- Each permission check queries database
- Could impact performance at scale
- No query result caching

❌ **Limited Direct User-Permission Assignment**
- Only role-based permission assignment supported
- No direct user-permission bypass (all through roles)

❌ **No Audit Logging**
- Who assigned permissions to whom? When?
- No change history tracking

❌ **ABAC Limitations**
- Conditions use string-based value comparisons
- Type safety could be improved
- Complex boolean logic (deep AND/OR nesting) not explicitly addressed

---

## 8. RECOMMENDATIONS

### High Priority
1. **Create RolePermissionAssignmentController**
   - Enable dynamic role-permission management from frontend
   - Mirror RolePolicyAssignmentController pattern

2. **Create UserRoleAssignmentController**
   - Enable user-role assignments through API
   - Support bulk operations

3. **Add Audit Logging**
   - Track all RBAC/ABAC changes
   - Store who changed what and when

### Medium Priority
4. **Implement Permission Caching**
   - Cache user permissions for configurable duration
   - Implement cache invalidation on policy changes

5. **Create Comprehensive DTOs**
   - `RoleWithPermissionsDto` showing assigned + available
   - `UserWithRolesDto` showing assigned + available roles

6. **Add ABAC Testing Suite**
   - Unit tests for condition evaluation
   - Policy rule priority tests
   - Complex AND/OR logic tests

### Low Priority
7. **Enhance ABAC Type Safety**
   - Validate condition values against attribute DataTypes
   - Provide better operator-to-datatype matching

8. **Add Permission Refresh Endpoint**
   - Allow token refresh with updated permissions
   - Without requiring re-login

---

## 9. PERMISSION NAMING CONVENTIONS

**Observed Patterns:**
- Action + Entity format: `CreateUser`, `EditRole`, `DeletePermission`
- Action + List format: `ViewUserList`, `ViewRoleList`, `ViewPermissionList`
- Direct assignment formats: `AssignPolicyToRole`, `RemovePolicyFromRole`

**Suggested Standardization:**
- **Create**: `Create{Entity}` → CreateUser, CreateRole, CreatePermission
- **Read**: `View{Entity}` → ViewUser, ViewRole
- **List**: `View{Entity}List` → ViewUserList, ViewRoleList
- **Update**: `Edit{Entity}` → EditUser, EditRole
- **Delete**: `Delete{Entity}` → DeleteUser, DeleteRole
- **Assignment**: `{Action}{Entity}` → AssignRoleToUser, RemoveRoleFromUser

---

## 10. QUICK REFERENCE: KEY CLASSES & METHODS

### Critical Methods
- **UserRepository.HasPermissionAsync()** - Core RBAC check
- **ABACService.EvaluateAccessAsync()** - Full authorization with ABAC
- **AuthService.LoginAsync()** - Authentication + token generation
- **RoleService.AddPermissionsToRoleAsync()** - Assign permissions to roles (service only, no endpoint)
- **UserService.AddRoleToUserAsync()** - Assign roles to users (service only, no endpoint)

### Critical DTOs
- **LoginResultDto** - Login response with tokens
- **UserDto** - User with roles/permissions
- **RoleDto** - Role definition
- **PermissionDto** - Permission definition
- **ABACEvaluationResponseDto** - Detailed authorization audit

### Critical Entities
- **User, Role, Permission, UserRole, RolePermission** - RBAC core
- **PolicyRule, PolicyCondition, AttributeDefinition, ConditionOperator** - ABAC core

---

## Conclusion

The WFT.Infra backend implements a **sophisticated RBAC+ABAC hybrid system** with solid architectural foundations. The main limitation is the absence of API endpoints for dynamic role-permission and user-role management, which prevents frontend applications from fully managing the authorization model. Adding these missing controller endpoints and DTOs would complete the implementation and enable a fully functional frontend authorization management interface.



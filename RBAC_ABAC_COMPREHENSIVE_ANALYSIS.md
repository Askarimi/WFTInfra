# Comprehensive RBAC & ABAC Implementation Analysis

**Date**: October 24, 2025  
**Project**: WFT.Infra Backend  
**Scope**: Backend Role-Based Access Control (RBAC) and Attribute-Based Access Control (ABAC)

---

## 1. ENTITIES & RELATIONSHIPS

### 1.1 Core RBAC Entities

#### **User**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Key Properties**:
  - `Id` (long): Primary identifier
  - `Username` (string): Unique username
  - `Email` (string): User email
  - `FirstName`, `LastName` (optional strings)
  - `IsActive` (bool): Activation status
  - `LastLoginAt` (DateTime?): Last login timestamp
- **Navigation**: 
  - `UserRoles`: Many-to-many relationship with roles
  - `UserPassword`: One-to-one relationship

#### **Role**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Key Properties**:
  - `Id` (long): Primary identifier
  - `Name` (string, 100 chars max, unique): Role name (e.g., "Admin", "Manager")
  - `Description` (string, 500 chars max): Role description
  - `IsActive` (bool): Activation status (default: true)
- **Navigation**:
  - `UserRoles`: Collection of user-role assignments (one-to-many)
  - `RolePermissions`: Collection of role-permission links (one-to-many)
  - `RolePolicyRules`: Collection of ABAC policy assignments (one-to-many)
- **Database Index**: Unique index on `Name` for performance

#### **Permission**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Key Properties**:
  - `Id` (long): Primary identifier
  - `Name` (string): Permission code (e.g., "EditUser", "DeleteRole")
  - `DisplayName` (string): Localized display name (supports Persian)
  - `IsActive` (bool): Activation status
- **Navigation**:
  - `RolePermissions`: Collection of role-permission assignments (one-to-many)

#### **UserRole** (Junction Table)
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Implements many-to-many relationship between User and Role
- **Key Properties**:
  - `Id` (long)
  - `UserId` (long, FK)
  - `RoleId` (long, FK)
- **Constraints**:
  - Cascade delete on both foreign keys
  - Indexes on both `UserId` and `RoleId`

#### **RolePermission** (Junction Table)
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Implements many-to-many relationship between Role and Permission
- **Key Properties**:
  - `Id` (long)
  - `RoleId` (long, FK)
  - `PermissionId` (long, FK)
- **Constraints**:
  - Composite unique index on `(RoleId, PermissionId)` to prevent duplicates
  - Cascade delete behavior
  - Individual indexes on `RoleId` and `PermissionId`

### 1.2 ABAC Entities

#### **PolicyRule**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Defines fine-grained attribute-based policy rules
- **Key Properties**:
  - `Id` (long)
  - `Name` (string): Policy identifier (e.g., "DepartmentDocumentAccess")
  - `DisplayName` (string): Localized display name
  - `Description` (string?): Optional detailed description
  - `IsActive` (bool): Activation status (default: true)
  - `Priority` (int, default: 1): Execution order (1 = highest priority)
  - `Effect` (string): "Allow" or "Deny" — determines policy outcome
- **Navigation**:
  - `PolicyConditions`: Collection of conditions (one-to-many)
  - `RolePolicyRules`: Collection of role-policy assignments (one-to-many)

#### **PolicyCondition**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Individual conditions that must be evaluated for a policy
- **Key Properties**:
  - `Id` (long)
  - `PolicyRuleId` (long, FK): Parent policy
  - `AttributeDefinitionId` (long, FK): Attribute to check
  - `ConditionOperatorId` (long, FK): Comparison operator (e.g., "Equals", "GreaterThan")
  - `Value` (string): Expected/comparison value
  - `LogicalOperator` (string): "AND" or "OR" — combines conditions
  - `Order` (int): Evaluation order within the policy
- **Navigation**:
  - `PolicyRule`: Parent policy
  - `AttributeDefinition`: Reference attribute
  - `ConditionOperator`: Reference operator

#### **AttributeDefinition**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Defines reusable attributes used in ABAC conditions
- **Key Properties**:
  - `Id` (long)
  - `Name` (string): Attribute code (e.g., "Department", "DocumentStatus")
  - `DisplayName` (string): Localized display name
  - `DataType` (string): "String", "Number", "Boolean", "DateTime"
  - `Source` (string): "User", "Resource", "Environment", "Action" — determines where the attribute value comes from
  - `IsRequired` (bool): Whether this attribute must exist
  - `DefaultValue` (string?): Fallback value if not provided
  - `IsActive` (bool): Activation status
  - `Description` (string?)
  - `AttributeGroupId` (long?, FK): Logical grouping
- **Navigation**:
  - `AttributeGroup`: Parent group
  - `AttributeValues`: Collection of values (one-to-many)
  - `PolicyConditions`: Collection of policies using this attribute

#### **AttributeValue**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Stores actual attribute values for users/resources
- **Key Properties**:
  - `Id` (long)
  - `AttributeDefinitionId` (long, FK)
  - `Value` (string): The actual attribute value
  - Related metadata (timestamps, entity references)

#### **AttributeGroup**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Logical grouping of related attributes
- **Key Properties**:
  - `Id` (long)
  - `Name` (string): Group name (e.g., "UserAttributes", "DocumentAttributes")
  - `DisplayName` (string)
- **Navigation**:
  - `AttributeDefinitions`: Collection (one-to-many)

#### **ConditionOperator**
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Defines comparison operators for policy conditions
- **Key Properties**:
  - `Id` (long)
  - `Name` (string): Operator code (e.g., "Equals", "Contains", "GreaterThan", "StartsWith")
  - `DisplayName` (string): Localized display name

#### **RolePolicyRule** (Junction Table)
- **Namespace**: `WFT.Infra.Core.Entities.UserManagment`
- **Purpose**: Assigns ABAC policies to roles
- **Key Properties**:
  - `Id` (long)
  - `RoleId` (long, FK)
  - `PolicyRuleId` (long, FK)
  - `IsActive` (bool): Can disable policy without deleting (default: true)
  - `ValidFrom` (DateTime?): Policy activation date
  - `ValidTo` (DateTime?): Policy expiration date
- **Constraints**: Cascade delete behavior

### 1.3 Data Flow: User → Roles → Permissions

```
User (1) ──→ UserRole (Many) ──→ Role (1) 
                                    ↓
                            RolePermission (Many)
                                    ↓
                               Permission (1)
```

**Trace Example**:
- User "Ali" has ID = 5
- `UserRole` entries: (UserId=5, RoleId=2), (UserId=5, RoleId=3)
- Role 2: "Manager"; Role 3: "Accountant"
- `RolePermission` entries: (RoleId=2, PermissionId=10), (RoleId=2, PermissionId=11), (RoleId=3, PermissionId=11)
- Permission 10: "EditUser"; Permission 11: "ViewReports"
- **Result**: Ali has permissions: EditUser, ViewReports

---

## 2. REPOSITORIES & SERVICES

### 2.1 Repositories

#### **IUserRepository** (Custom Interface)
- **Namespace**: `WFT.Infra.Application.Contracts.Repositories`
- **Key Methods**:
  - `GetByIdWithRolesAndPermissionsAsync(long userId)`: Loads complete user with all roles and permissions (eager loading)
  - `HasPermissionAsync(long userId, string permissionName)`: Efficiently checks if user has a specific permission
    - **Implementation**: Joins across `UserRole → Role → RolePermission → Permission`
    - **Normalization**: Case-insensitive comparison, trims whitespace
    - **Filtering**: Only considers active permissions

#### **IRepository<User>** (Generic Base)
- **Namespace**: `WFT.Infra.Application.Contracts.Repositories`
- Standard CRUD operations via `IServiceBase<User>`

#### **IRepository<Role>** (Generic Base)
- Standard CRUD operations for role management

#### **IRepository<Permission>** (Generic Base)
- Standard CRUD operations for permission management

#### **IRepository<UserRole>** (Generic Base)
- Direct junction table access for relationship management

#### **IRepository<RolePermission>** (Generic Base)
- Direct junction table access for role-permission associations

### 2.2 Application Services

#### **IUserService**
- **Namespace**: `WFT.Infra.Application.Contracts.Interfaces.UserManagment`
- **Key Methods**:
  - `GetRolesForUserAsync(long userId)` → `IEnumerable<RoleDto>`: Retrieves all roles assigned to a user
  - `AddRoleToUserAsync(long userId, List<long> roleIds)`: Assigns multiple roles to a user
  - `HasPermissionAsync(long userId, string permissionName)` → `bool`: Delegates to `IUserRepository.HasPermissionAsync()`
  - Standard CRUD: `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetByIdAsync`
  - Pagination: `GetPagedListAsync(PagedQueryRequest)`
  - Search: `GetByUsernameAsync()`, `GetByNameAsync()`

#### **IRoleService**
- **Namespace**: `WFT.Infra.Application.Contracts.Interfaces.UserManagment`
- **Key Methods**:
  - `AddPermissionsToRoleAsync(long roleId, List<long> permissionIds)`: Bulk assign permissions to a role
  - `RemovePermissionsFromRoleAsync(long roleId, List<long> permissionIds)`: Revoke permissions from a role
  - `GetPermissionsForRoleAsync(long roleId)` → `IEnumerable<PermissionDto>`: Get all permissions for a specific role
  - `GetPermissionsForRoleAsync(List<long> roleIds)` → `IEnumerable<PermissionDto>`: Get all permissions for multiple roles
  - `GetActiveRolesAsync()` → `IEnumerable<RoleDto>`: List only active roles
  - Standard CRUD: `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetByIdAsync`
  - Pagination & Search

#### **IPermissionService**
- **Namespace**: `WFT.Infra.Application.Contracts.Interfaces.UserManagment`
- **Key Methods**:
  - `GetActivePermissionsAsync()` → `IEnumerable<PermissionDto>`: List only active permissions
  - Standard CRUD and pagination via `IServiceBase<PermissionDto>`

#### **IABACService**
- **Namespace**: `WFT.Infra.Application.Contracts.Interfaces.UserManagment`
- **Purpose**: Implements ABAC evaluation logic
- **Key Methods**:
  - `EvaluateAccessAsync(long userId, string permission, object? resource = null, object? context = null)` → `bool`
    - **Flow**:
      1. Check RBAC permission (fast path)
      2. Retrieve applicable ABAC policies for user's roles
      3. Evaluate each policy in priority order
      4. Return access decision based on Effect (Allow/Deny)
    - **Default**: Deny if no matching policy
  - `EvaluateAccessDetailedAsync(...)` → `ABACEvaluationResponseDto`
    - **Returns**: Comprehensive evaluation with:
      - User attributes and resource attributes
      - Each policy evaluation result
      - Condition-by-condition breakdown
      - Reason for access decision (supports Persian localization)
  - `GetApplicablePoliciesAsync(long userId, string permission)` → `IEnumerable<PolicyRuleDto>`
    - Retrieves all ABAC policies assigned to user's roles
  - `GetUserAttributesAsync(long userId)` → `IEnumerable<AttributeValueDto>`
    - Fetches all attributes assigned to the user
  - `ValidatePolicyRuleAsync(PolicyRuleDto, object? resource, object? context)` → `bool`
    - Evaluates all conditions in a policy

#### **IPolicyRuleService**
- **Namespace**: `WFT.Infra.Application.Contracts.Interfaces.UserManagment`
- **Key Methods**:
  - `GetPoliciesForRoleAsync(long roleId)` → `IEnumerable<PolicyRuleDto>`: Policies assigned to a role
  - `GetPoliciesForPermissionAsync(string permission)` → `IEnumerable<PolicyRuleDto>`: Policies for a specific permission
  - `AddPolicyToRoleAsync(long roleId, long policyRuleId)` → `bool`: Assign policy to role
  - `RemovePolicyFromRoleAsync(long roleId, long policyRuleId)` → `bool`: Revoke policy from role
  - `GetActivePoliciesAsync()` → `IEnumerable<PolicyRuleDto>`: List only active policies
  - Standard CRUD and pagination

#### **IAttributeService**
- **Purpose**: Manages ABAC attribute definitions and values
- **Key Methods**:
  - `GetUserAttributesAsync(long userId)` → `IEnumerable<AttributeValueDto>`
  - `GetResourceAttributesAsync(long resourceId, string resourceType)` → `IEnumerable<AttributeValueDto>`
  - `GetAttributeValueAsync(long userId, string attributeName)` → `AttributeValueDto?`

#### **IConditionOperatorService**
- **Purpose**: Manages condition operators (Equals, Contains, GreaterThan, etc.)
- **Key Methods**:
  - `GetActiveOperatorsAsync()` → `IEnumerable<ConditionOperatorDto>`

#### **IAuthorizationService**
- **Namespace**: `WFT.Infra.Application.Contracts.Interfaces`
- **Purpose**: High-level authorization orchestration
- **Key Methods**:
  - `HasPermissionAsync(long userId, string permissionName, object? resource = null)` → `bool`
    - **Flow**: 
      1. Call `IUserService.HasPermissionAsync()` for RBAC check
      2. If resource provided, call `IABACService.EvaluateAccessAsync()` for ABAC check
      3. Return combined result
  - `EvaluateAccessDetailedAsync(long userId, string permissionName, object? resource = null, object? context = null)` → `ABACEvaluationResponseDto`
    - Returns detailed evaluation results (primarily for debugging and logging)

#### **AuthService** (Authentication)
- **Namespace**: `WFT.Infra.Application.Services`
- **Purpose**: Handles user login and registration
- **Key Method: LoginAsync(UserLoginDto)**
  - **Flow**:
    1. Validate username/password via `IUserPasswordService`
    2. Retrieve user's roles via `IUserService.GetRolesForUserAsync()`
    3. Retrieve permissions via `IRoleService.GetPermissionsForRoleAsync(roleIds)`
    4. Generate JWT token with roles and permissions as claims
    5. Return `LoginResultDto` containing:
       - Access token (JWT)
       - Refresh token
       - User object (with Roles and Permissions lists)

### 2.3 Permission Resolution Flow

**Step-by-step trace for checking if User has "EditUser" permission**:

```
HasPermissionAsync(userId=5, "EditUser")
  ↓
IUserRepository.HasPermissionAsync(5, "EditUser")
  ↓
SELECT EXISTS (
  SELECT 1 FROM [Users] u
  JOIN [UserRoles] ur ON u.Id = ur.UserId
  JOIN [Roles] r ON ur.RoleId = r.Id
  JOIN [RolePermissions] rp ON r.Id = rp.RoleId
  JOIN [Permissions] p ON rp.PermissionId = p.Id
  WHERE u.Id = 5
    AND p.IsActive = 1
    AND LOWER(p.Name) = 'edituser'
)
  ↓
Returns: true/false
```

---

## 3. CONTROLLERS & API ENDPOINTS

### 3.1 **UsersController**
- **Base Route**: `/api/users`
- **Purpose**: User management with authorization checks

| Method | Endpoint | Permission Required | Function |
|--------|----------|-------------------|----------|
| POST | `/Add` | `CreateUser` | Create new user |
| GET | `/init/{id:long}` | `ViewUser` | Get user by ID |
| GET | `/List` | `ViewUserList` | List users (paginated) |
| PUT | `/update` | `EditUser` | Update user details |
| DELETE | `/delete/{id:long}` | `DeleteUser` | Delete user |

**Authorization**: Each endpoint calls `IAuthorizationService.HasPermissionAsync()` before processing.

### 3.2 **RolesController**
- **Base Route**: `/api/roles`
- **Purpose**: Role CRUD operations

| Method | Endpoint | Permission Required | Function |
|--------|----------|-------------------|----------|
| POST | `/Add` | `CreateRole` | Create new role |
| GET | `/init/{id:long}` | `ViewRole` | Get role by ID |
| GET | `/List` | `ViewRoleList` | List roles (paginated) |
| PUT | `/update` | `EditRole` | Update role |
| DELETE | `/delete/{id:long}` | `DeleteRole` | Delete role |

### 3.3 **PermissionsController**
- **Base Route**: `/api/permissions`
- **Purpose**: Permission management

| Method | Endpoint | Permission Required | Function |
|--------|----------|-------------------|----------|
| POST | `/Add` | `CreatePermission` | Create new permission |
| GET | `/init/{id:long}` | `ViewPermission` | Get permission by ID |
| GET | `/List` | `ViewPermissionList` | List permissions (paginated) |
| PUT | `/update` | `EditPermission` | Update permission |
| DELETE | `/delete/{id:long}` | `DeletePermission` | Delete permission |

### 3.4 **RolePolicyAssignmentController**
- **Base Route**: `/api/rolepolicyassignment`
- **Purpose**: Assign ABAC policies to roles

| Method | Endpoint | Permission Required | Function |
|--------|----------|-------------------|----------|
| POST | `/assign/{roleId:long}/{policyRuleId:long}` | `AssignPolicyToRole` | Assign policy to role |
| DELETE | `/remove/{roleId:long}/{policyRuleId:long}` | `RemovePolicyFromRole` | Remove policy from role |
| GET | `/role/{roleId:long}` | `ViewRolePolicyAssignment` | Get policies for a role |
| POST | `/bulkassign/{roleId:long}` | `AssignPolicyToRole` | Assign multiple policies |
| DELETE | `/bulkremove/{roleId:long}` | `RemovePolicyFromRole` | Remove multiple policies |

### 3.5 **PolicyRulesController**
- **Base Route**: `/api/policyrules`
- **Purpose**: ABAC policy management

| Method | Endpoint | Permission Required | Function |
|--------|----------|-------------------|----------|
| POST | `/Add` | Implied | Create policy rule |
| GET | `/List` | Implied | List policies |
| GET | `/init/{id:long}` | Implied | Get policy details |
| PUT | `/update` | Implied | Update policy |
| DELETE | `/delete/{id:long}` | Implied | Delete policy |

### 3.6 **AttributeDefinitionsController**
- **Base Route**: `/api/attributedefinitions`
- **Purpose**: Manage ABAC attributes

### 3.7 **AttributeGroupsController**
- **Base Route**: `/api/attributegroups`
- **Purpose**: Organize attributes into logical groups

### 3.8 **ConditionOperatorsController**
- **Base Route**: `/api/conditionoperators`
- **Purpose**: Manage comparison operators for conditions

### 3.9 **ABACController**
- **Base Route**: `/api/abac`
- **Purpose**: ABAC evaluation and testing
- **Key Endpoint**: POST `/evaluate` — Test policy evaluation with request body

### 3.10 **AuthController**
- **Base Route**: `/api/auth`
- **Purpose**: Authentication (login, register, logout)

| Method | Endpoint | Auth Required | Function |
|--------|----------|---------------|----------|
| POST | `/register` | No | Register new user |
| POST | `/login` | No | Authenticate user |
| POST | `/logout` | Yes | Revoke refresh token |

---

## 4. PERMISSION EVALUATION MECHANISM

### 4.1 RBAC Evaluation (Fast Path)

**Entry Point**: `IAuthorizationService.HasPermissionAsync(userId, permissionName, resource?)`

**Process**:
1. **Check RBAC permission first** (always executed):
   ```csharp
   var hasPermission = await _userService.HasPermissionAsync(userId, permissionName);
   if (!hasPermission) return false; // Fast rejection
   ```

2. **ABAC evaluation** (optional, only if resource provided):
   ```csharp
   if (resource != null)
   {
       return await _abacService.EvaluateAccessAsync(userId, permissionName, resource);
   }
   ```

3. **Default Allow**: If RBAC passes and no ABAC policies, allow access

**Database Query Optimization**:
- Single SQL query with JOINs (no N+1 queries)
- Case-insensitive, whitespace-trimmed comparison
- Only evaluates active permissions and roles

### 4.2 ABAC Evaluation (Detailed Path)

**Entry Point**: `IABACService.EvaluateAccessAsync(...)`

**Algorithm**:

```
1. Verify RBAC permission passes (baseline requirement)
   ↓ If fails, return false
   
2. Load all ABAC policies for user's roles
   ↓ Query: Role → RolePolicyRule → PolicyRule (filtered by role, active only)
   
3. Sort policies by Priority (1 = highest)
   
4. For each policy:
   a) Evaluate all PolicyConditions (joined by LogicalOperator: AND/OR)
   b) For each condition:
      - Get attribute value (from user, resource, environment, or action)
      - Get operator (Equals, Contains, GreaterThan, etc.)
      - Apply operator: attributeValue OP expectedValue
   c) If Effect = "Deny" AND conditions met → return false (deny)
   d) If Effect = "Allow" AND conditions met → return true (allow)
   
5. Default deny: return false (no policy granted access)
```

**Example Evaluation**:

Policy: "DepartmentDocumentAccess"
- Effect: "Allow"
- Priority: 1
- Conditions:
  1. Department = "Finance" (AND)
  2. DocumentStatus = "Approved" (AND)

Evaluation for user "Ali":
- Ali's Department attribute = "Finance" ✓
- Document's Status attribute = "Approved" ✓
- All conditions met + Effect=Allow → **Access Granted**

### 4.3 Detailed Evaluation Response

**Method**: `IABACService.EvaluateAccessDetailedAsync(...)`

**Response**: `ABACEvaluationResponseDto`

**Contains**:
- `HasAccess` (bool): Final decision
- `UserAttributes` (list): All user attributes evaluated
- `ResourceAttributes` (list): All resource attributes evaluated
- `PolicyResults` (list): Each policy with:
  - `PolicyName`, `Effect`, `Priority`
  - `ConditionResults`: Each condition with:
    - `AttributeName`, `Operator`, `ExpectedValue`, `ActualValue`
    - `IsMet` (bool): Whether condition passed
    - `Reason` (string): Persian localized explanation
- `EvaluationReason` (string): Persian localized final decision reason

**Use Case**: Debugging, logging, frontend feedback

### 4.4 Caching Strategy

**Current**: No explicit caching layer
- **Implications**:
  - Database hit on every permission check
  - **Performance Impact**: Acceptable for typical admin operations (rare)
  - **Future Optimization**: Cache user's permission set in Redis with TTL

---

## 5. INTEGRATION & FRONTEND GAPS

### 5.1 Login Response Payload

**Endpoint**: `POST /api/auth/login`

**Response Structure**:
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "base64encodedtoken",
    "user": {
      "id": 5,
      "username": "ali",
      "firstName": "Ali",
      "lastName": "Mohammadi",
      "email": "ali@example.com",
      "isActive": true,
      "lastLoginAt": "2025-10-24T12:00:00Z",
      "roles": ["Manager", "Accountant"],
      "permissions": ["EditUser", "ViewReports", "DeleteRole"]
    }
  },
  "isSuccess": true
}
```

**Key Points**:
- Roles and Permissions are **flat arrays of strings**
- Permissions are computed at login by traversing User → Roles → RolePermissions → Permissions
- No granular permission details (e.g., description, displayName)

### 5.2 JWT Token Claims

**Generated by**: `IJwtTokenGenerator.GenerateToken(userId, username, roles, permissions)`

**Claims Structure**:
```
[Standard Claims]
- sub (subject): userId as string
- unique_name: username

[Role Claims]
- role (claim type): One claim per role
  Example: role: "Manager", role: "Accountant"

[Permission Claims]
- permission (custom claim type): One claim per permission
  Example: permission: "EditUser", permission: "ViewReports"
```

**Verification**: During authorization, `PermissionHandler` reads `ClaimTypes.NameIdentifier` to get userId and calls `HasPermissionAsync()` backend validation.

### 5.3 Missing Functionality for Dynamic Frontend Management

#### **Gap 1: Missing Role-Permission Assignment Endpoints**
- ❌ No controller endpoint to assign/remove permissions to/from a role via REST API
- **Available**: Only internal service method `IRoleService.AddPermissionsToRoleAsync()`
- **Blocker**: Frontend cannot dynamically edit role permissions without backend changes
- **Solution Required**: Create `RolePermissionAssignmentController` with:
  - `POST /api/rolepermission/assign/{roleId}/{permissionId}`
  - `DELETE /api/rolepermission/remove/{roleId}/{permissionId}`
  - `GET /api/rolepermission/role/{roleId}` — list permissions for a role
  - `POST /api/rolepermission/bulkassign/{roleId}` — bulk assign

#### **Gap 2: Missing User-Role Assignment Endpoints**
- ❌ No REST API endpoint to assign/remove roles to/from a user
- **Available**: Only internal `IUserService.AddRoleToUserAsync()`
- **Blocker**: Frontend must be extended to manage user role assignments
- **Solution Required**: Create `UserRoleAssignmentController` with:
  - `POST /api/userrole/assign/{userId}/{roleId}`
  - `DELETE /api/userrole/remove/{userId}/{roleId}`
  - `GET /api/userrole/user/{userId}` — list roles for a user
  - `POST /api/userrole/bulkassign/{userId}` — bulk assign

#### **Gap 3: Permission Details Not Returned**
- ❌ Login response includes permission names only (strings)
- ❌ No access to `DisplayName` or other permission metadata
- **Impact**: Frontend cannot provide localized permission descriptions
- **Solution Required**: Extend `LoginResultDto.User.Permissions` to return `PermissionDto` objects instead of strings, or create separate endpoint:
  - `GET /api/users/{userId}/permissions` — returns detailed permission list

#### **Gap 4: No Direct User-Permission Assignment**
- ❌ Only role-based assignment supported
- ❌ No endpoint for direct user-specific permissions (bypass roles)
- **Impact**: Cannot implement granular, user-specific overrides
- **Note**: May be intentional design decision for simplicity

#### **Gap 5: ABAC Policy Conditions UI Not Provided**
- ✓ Backend supports full ABAC policy creation
- ❌ No controller actions for creating/editing `PolicyConditions` directly
- ❌ Policy creation must bundle all conditions
- **Impact**: Frontend cannot incrementally build policies
- **Solution Required**: Granular endpoints for conditions:
  - `POST /api/policyconditions/add`
  - `PUT /api/policyconditions/update/{id}`
  - `DELETE /api/policyconditions/delete/{id}`

### 5.4 Frontend Integration Points

**Authentication Flow**:
1. Frontend calls `POST /api/auth/login` → receives roles and permissions
2. Frontend stores roles/permissions in local state or localStorage
3. Frontend uses permissions for UI visibility decisions
4. Backend validates each API call via `IAuthorizationService.HasPermissionAsync()`

**Permission Synchronization**:
- ✓ Frontend receives full permission list at login
- ⚠️ Changes made on backend are not reflected until re-login
- **Implication**: No real-time permission updates across sessions
- **Mitigation**: Implement permission refresh endpoint

**ABAC Integration**:
- ⚠️ Backend evaluates ABAC policies; frontend unaware
- ❌ Frontend cannot display attribute-based restrictions to users
- **Impact**: Poor UX for resource-restricted access

### 5.5 Endpoints Summary Table

| Feature | Endpoint Exists | Notes |
|---------|-----------------|-------|
| Create Role | ✓ | `POST /api/roles/Add` |
| Edit Role | ✓ | `PUT /api/roles/update` |
| Delete Role | ✓ | `DELETE /api/roles/delete/{id}` |
| Assign Permission to Role | ❌ | **MISSING** |
| Remove Permission from Role | ❌ | **MISSING** |
| List Role Permissions | ❌ | **MISSING** |
| Assign Policy to Role | ✓ | `POST /api/rolepolicyassignment/assign/{roleId}/{policyId}` |
| Create Permission | ✓ | `POST /api/permissions/Add` |
| Edit Permission | ✓ | `PUT /api/permissions/update` |
| Delete Permission | ✓ | `DELETE /api/permissions/delete/{id}` |
| Assign Role to User | ❌ | **MISSING** |
| Remove Role from User | ❌ | **MISSING** |
| List User Roles | ❌ | **MISSING** |
| Get User Permissions | ❌ | **MISSING** (only at login) |
| Create ABAC Policy | ✓ | `POST /api/policyrules/Add` |
| Edit ABAC Policy | ✓ | `PUT /api/policyrules/update` |
| Evaluate ABAC (Debug) | ✓ | `POST /api/abac/evaluate` |

---

## 6. KEY FINDINGS & RECOMMENDATIONS

### 6.1 What's Working Well ✓

1. **Clean Architecture**: Clear separation between entities, repositories, services, and controllers
2. **RBAC Foundation**: Basic role-permission-user model is solid
3. **ABAC Framework**: Comprehensive ABAC implementation with PolicyRules, Conditions, and Attributes
4. **Localization**: Support for Persian (Farsi) display names and error messages
5. **Security**: RBAC validation on every API endpoint, case-insensitive permission checks
6. **Token Integration**: Roles and permissions embedded in JWT claims
7. **Database Design**: Proper indexes, composite unique constraints, cascade delete rules

### 6.2 Critical Gaps ⚠️

1. **No Dynamic Role-Permission Management API**: Cannot edit role permissions via REST API
2. **No Dynamic User-Role Management API**: Cannot assign roles to users via REST API
3. **Incomplete Login Response**: Permissions returned as strings only; missing metadata
4. **No Real-time Permission Updates**: Permission changes require user re-login
5. **ABAC Complexity**: Policy creation requires all conditions in single request; no granular endpoints

### 6.3 Performance Considerations

| Operation | Query Type | Performance | Notes |
|-----------|-----------|-------------|-------|
| Check Permission | Single JOIN query | ✓ Good | No N+1; uses aggregate query |
| Load User + Roles + Perms | Eager load | ✓ Good | Single query tree |
| Get Applicable ABAC Policies | Loop + JOIN | ⚠️ Acceptable | One query per role; could batch |
| Evaluate All Conditions | Loop | ⚠️ Acceptable | Sequential evaluation; no parallelization |

**Recommendations**:
- Cache user permission set in Redis (TTL: 5-15 minutes)
- Batch policy queries per user instead of per role
- Consider parallel condition evaluation for complex policies

### 6.4 Recommended Next Steps

**Priority 1 (Blockers)**:
- [ ] Create `RolePermissionAssignmentController` (CRUD endpoints for role-permission mappings)
- [ ] Create `UserRoleAssignmentController` (CRUD endpoints for user-role mappings)
- [ ] Update `LoginResultDto` to include detailed permission objects

**Priority 2 (Enhancements)**:
- [ ] Implement permission refresh endpoint for real-time updates
- [ ] Add granular `PolicyConditionController` for incremental policy building
- [ ] Implement caching layer for user permissions
- [ ] Add resource-scoped permission checks (e.g., "EditUser:UserID=5")

**Priority 3 (Future)**:
- [ ] Implement direct user-permission assignment (bypass roles)
- [ ] Add permission revocation audit logging
- [ ] Create ABAC policy templates
- [ ] Implement time-based access restrictions (ValidFrom/ValidTo)

### 6.5 Security Observations

✓ **Strong**:
- RBAC validated on every endpoint
- ABAC provides fine-grained control
- JWT includes roles and permissions
- Case-insensitive permission matching prevents bypass

⚠️ **Review**:
- Default ABAC policy deny (correct security posture)
- Refresh token stored in HttpOnly cookie (good)
- No explicit rate limiting on permission checks

---

## 7. DATABASE SCHEMA OVERVIEW

**Tables**:
- `Users`: User accounts
- `Roles`: Role definitions
- `Permissions`: Permission definitions
- `UserRoles`: Many-to-many junction (User ↔ Role)
- `RolePermissions`: Many-to-many junction (Role ↔ Permission)
- `PolicyRules`: ABAC policy definitions
- `PolicyConditions`: ABAC conditions within policies
- `AttributeDefinitions`: ABAC attribute definitions
- `AttributeValues`: Actual attribute values (user/resource)
- `AttributeGroups`: Logical attribute groupings
- `ConditionOperators`: Comparison operators
- `RolePolicyRules`: Many-to-many junction (Role ↔ PolicyRule)

**Indexes**:
- `Role.Name`: Unique index (0.1ms lookup)
- `UserRole(UserId, RoleId)`: Composite index
- `RolePermission(RoleId, PermissionId)`: Composite unique index

---

## 8. CONCLUSION

The WFT Infrastructure backend implements a **sophisticated RBAC + ABAC system** suitable for complex authorization scenarios. The core architecture is sound, but the **frontend integration is incomplete** — critical CRUD endpoints for role-permission and user-role management are missing, preventing dynamic runtime configuration.

**Immediate Action**: Implement the three "Priority 1" items above to enable frontend-driven authorization management.

---

**Report Generated**: October 24, 2025  
**Analyzed Codebase**: WFT.Infra Solution (6 projects)  
**Scopes Covered**: Entities, Repositories, Services, Controllers, Authentication, ABAC Evaluation



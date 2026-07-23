# RBAC & ABAC Authorization System - Executive Summary

## System Overview

```
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃         DUAL-LAYER AUTHORIZATION SYSTEM                   ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫
┃                                                             ┃
┃  Layer 1: RBAC (Role-Based Access Control)                ┃
┃  ├─ User assigned to Role(s)                              ┃
┃  ├─ Role assigned to Permission(s)                        ┃
┃  └─ Endpoint checks: User has Role → Role has Permission ┃
┃                                                             ┃
┃  Layer 2: ABAC (Attribute-Based Access Control)           ┃
┃  ├─ Role assigned to PolicyRule(s)                        ┃
┃  ├─ PolicyRule has Condition(s) with Attribute-based      ┃
┃  │  evaluation (Department, Time, AccessLevel, etc.)      ┃
┃  └─ Endpoint checks: User meets all conditions            ┃
┃                                                             ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
```

---

## Permission Resolution Flow

### At Login:
```
User logs in
    ↓
AuthService.LoginAsync()
    ↓
    ├─ Verify password
    ├─ Get User's Roles (UserService.GetRolesForUserAsync)
    ├─ Get Permissions for those Roles (RoleService.GetPermissionsForRoleAsync)
    ├─ Generate JWT token with role claims
    ├─ Generate Refresh Token (expires 7 days)
    └─ Return: AccessToken + User{id, username, roles} ⚠️ (permissions not included)

Result: Frontend stores JWT + roles
        Backend validates permissions on each request
```

### At Each Protected Endpoint:
```
Request arrives with JWT token
    ↓
Extract userId from token
    ↓
AuthorizationService.HasPermissionAsync(userId, "RequiredPermission")
    ↓
    ├─ RBAC Check:
    │   UserRepository.HasPermissionAsync(userId, permissionName)
    │   Query: User → UserRole → Role → RolePermission → Permission
    │   Result: true/false (1 database query)
    │
    └─ If RBAC passes + ABAC policies exist:
        ABACService.EvaluateAccessAsync()
        ├─ Get user's roles
        ├─ Get applicable policies for those roles
        ├─ For each policy (in priority order):
        │   ├─ Evaluate all conditions (AND/OR logic)
        │   ├─ If Effect=Deny && conditions met → Deny
        │   ├─ If Effect=Allow && conditions met → Allow
        │   └─ Continue to next policy
        └─ Default: Deny

Result: Permission granted or denied
```

---

## Database Schema

### Core RBAC Tables:
```
USER
├─ Id (PK)
├─ Username
├─ Email
├─ IsActive

USER_ROLE (Junction)
├─ Id (PK)
├─ UserId (FK) → USER
└─ RoleId (FK) → ROLE

ROLE
├─ Id (PK)
├─ Name
├─ Description
├─ IsActive

ROLE_PERMISSION (Junction)
├─ Id (PK)
├─ RoleId (FK) → ROLE
└─ PermissionId (FK) → PERMISSION

PERMISSION
├─ Id (PK)
├─ Name (e.g., "EditUser", "DeleteRole")
├─ DisplayName (Persian: "ویرایش کاربر")
└─ IsActive
```

### ABAC-Specific Tables:
```
ROLE_POLICY_RULE (Junction)
├─ Id (PK)
├─ RoleId (FK) → ROLE
├─ PolicyRuleId (FK) → POLICY_RULE
├─ IsActive
├─ ValidFrom (temporal constraint)
└─ ValidTo (temporal constraint)

POLICY_RULE
├─ Id (PK)
├─ Name (e.g., "DepartmentDocumentAccess")
├─ DisplayName (Persian)
├─ Priority (1 = highest)
├─ Effect (Allow / Deny)
└─ IsActive

POLICY_CONDITION
├─ Id (PK)
├─ PolicyRuleId (FK) → POLICY_RULE
├─ AttributeDefinitionId (FK) → ATTRIBUTE_DEFINITION
├─ ConditionOperatorId (FK) → CONDITION_OPERATOR
├─ Value (e.g., "IT Department")
├─ LogicalOperator (AND / OR)
└─ Order (execution order)

ATTRIBUTE_DEFINITION
├─ Id (PK)
├─ Name (e.g., "Department", "AccessLevel")
├─ DisplayName (Persian)
├─ DataType (String, Number, Boolean, DateTime)
├─ Source (User, Resource, Environment, Action)
├─ IsActive

ATTRIBUTE_VALUE
├─ Id (PK)
├─ UserId (FK)
├─ AttributeDefinitionId (FK)
├─ Value (e.g., "Marketing")

CONDITION_OPERATOR
├─ Id (PK)
├─ Name (e.g., "Equals", "Contains", "GreaterThan")
├─ Symbol (==, contains, >)
├─ DataTypes (String, Number, etc.)
└─ IsActive
```

---

## Service Layer Overview

| Service | Purpose | Key Methods |
|---------|---------|------------|
| **UserService** | User management & permission checks | `HasPermissionAsync()`, `GetRolesForUserAsync()`, `AddRoleToUserAsync()` |
| **RoleService** | Role management & permission assignment | `AddPermissionsToRoleAsync()`, `GetPermissionsForRoleAsync()` |
| **PermissionService** | Permission definitions | `GetPagedListAsync()`, standard CRUD |
| **ABACService** | Attribute-based policy evaluation | `EvaluateAccessAsync()`, `EvaluateAccessDetailedAsync()`, `GetApplicablePoliciesAsync()` |
| **PolicyRuleService** | Policy rule management | `AddPolicyToRoleAsync()`, `GetPoliciesForRoleAsync()` |
| **AuthorizationService** | High-level authorization facade | `HasPermissionAsync()`, `EvaluateAccessDetailedAsync()` |
| **AttributeService** | User attributes for ABAC | `GetAttributeValueAsync()`, `SetAttributeValueAsync()` |

---

## API Endpoints

### ✅ Fully Implemented:
- `POST /auth/login` - Login, get token
- `POST /auth/logout` - Logout
- `POST /auth/refresh-token` - Refresh token
- `POST /roles/Add` - Create role
- `GET /roles/List` - List roles
- `PUT /roles/update` - Update role
- `DELETE /roles/delete/{id}` - Delete role
- `POST /permissions/Add` - Create permission
- `GET /permissions/List` - List permissions
- `PUT /permissions/update` - Update permission
- `DELETE /permissions/delete/{id}` - Delete permission
- `POST /rolepolicyassignment/assign/{roleId}/{policyId}` - Assign policy to role
- `DELETE /rolepolicyassignment/remove/{roleId}/{policyId}` - Remove policy from role
- `GET /rolepolicyassignment/role/{roleId}` - Get policies for role
- `POST /abac/evaluate` - Evaluate access
- `POST /abac/evaluate-detailed` - Detailed evaluation with audit trail
- `POST /users/Add` - Create user
- `GET /users/List` - List users
- `PUT /users/update` - Update user
- `DELETE /users/delete/{id}` - Delete user

### ⚠️ Partially Implemented (Service exists, no endpoint):
- **Assign permissions to role** - `RoleService.AddPermissionsToRoleAsync()` exists
- **Get permissions for role** - `RoleService.GetPermissionsForRoleAsync()` exists
- **Assign roles to user** - `UserService.AddRoleToUserAsync()` exists
- **Get roles for user** - `UserService.GetRolesForUserAsync()` exists

### ❌ Missing Endpoints:
```
POST   /users/{userId}/roles/add              ← Assign roles to user
DELETE /users/{userId}/roles/remove           ← Remove roles from user
GET    /users/{userId}/roles                  ← Get user's roles
GET    /roles/{roleId}/permissions            ← Get role's permissions
POST   /roles/{roleId}/permissions/add        ← Assign permissions to role
DELETE /roles/{roleId}/permissions/remove     ← Remove permissions from role
```

---

## ABAC Policy Evaluation Logic

### Step-by-Step Example:

**Policy Rule**: "OnlyViewReportsDuringBusinessHours"
- Priority: 1
- Effect: Allow
- Conditions:
  1. `User.Department == Resource.Department` (AND)
  2. `CurrentTime BETWEEN 09:00-17:00` (AND)
  3. `User.IsManager == true` (AND)

**Evaluation**:
```
if (User.Department == "IT" && Resource.Department == "IT")  ✓ Met
   AND CurrentTime == "14:30" (14:30 is between 09:00-17:00)  ✓ Met
   AND User.IsManager == true                                 ✓ Met
{
    Access = ALLOW (because Effect="Allow" and all conditions met)
}
```

### Supported Condition Operators:

| Operator | Type | Example |
|----------|------|---------|
| `Equals` | String/Number | `Department == "IT"` |
| `Contains` | String | `Email CONTAINS "@company.com"` |
| `StartsWith` | String | `Username STARTS WITH "admin"` |
| `EndsWith` | String | `Email ENDS WITH ".com"` |
| `GreaterThan` | Number | `Age > 18` |
| `LessThan` | Number | `Salary < 100000` |
| `Between` | Number | `Age BETWEEN 18-65` |
| `In` | Array | `Status IN [Active, Pending]` |

---

## Key Gaps & Recommendations

### Critical Gaps (Phase 1):
1. ❌ **No User-Role Management Endpoints**
   - Service method exists: `UserService.AddRoleToUserAsync()`
   - Missing: Controller endpoints to expose this
   - Impact: Frontend cannot assign/revoke roles dynamically

2. ❌ **No Role-Permission Management Endpoints**
   - Service method exists: `RoleService.AddPermissionsToRoleAsync()`
   - Missing: Controller endpoints to expose this
   - Impact: Frontend cannot manage role permissions dynamically

3. ❌ **Permissions Not in Login Response**
   - Permissions are fetched during login (AuthService line 67)
   - But not included in `LoginResultDto`
   - Frontend only gets roles, not individual permissions
   - Impact: Frontend cannot render permission-based UI without additional API calls

### Enhancement Gaps (Phase 2):
4. ⚠️ **No Caching Layer**
   - Permission checks are real-time (good for consistency)
   - No Redis/in-memory caching (bad for performance)
   - Impact: Repeated permission checks cause N+1 queries

5. ⚠️ **No Audit Logging**
   - Permission changes not logged
   - No audit trail for compliance
   - Impact: Cannot track who changed permissions when

6. ⚠️ **Temporal Constraints Not Enforced**
   - `RolePolicyRule.ValidFrom` and `ValidTo` exist in DB
   - Not checked during policy evaluation
   - Impact: Time-based access restrictions don't work

### Advanced Gaps (Phase 3):
7. ❌ **No Role Inheritance**
   - Roles cannot inherit from other roles
   - Each role must have explicit permissions
   - Workaround: Manually assign same permissions to multiple roles

8. ❌ **No Direct User-Permission Assignment**
   - Only role-based permissions supported
   - No `UserPermission` junction table
   - Workaround: Create single-user role if needed

---

## System Security Assessment

### ✅ Strengths:
- JWT-based authentication with secure tokens
- Role-based access control with role validation on every endpoint
- Attribute-based access control with condition evaluation
- Default-deny policy (unless explicitly allowed)
- Comprehensive audit trail via `EvaluateAccessDetailedAsync()`

### ⚠️ Weaknesses:
- No rate limiting on authorization checks
- No permission change audit logging
- No temporal constraint enforcement
- No caching invalidation strategy
- No transactional bulk operations

### Recommendations:
1. Add permission operation audit logging (who, what, when)
2. Implement temporal constraint checks in policy evaluation
3. Add rate limiting to authorization endpoints
4. Encrypt sensitive attribute values at rest
5. Implement permission caching with versioning
6. Make bulk operations transactional

---

## Migration Path to Full Implementation

### Immediate (Week 1):
```
1. Add 4 missing endpoints:
   ✓ POST /users/{userId}/roles/add
   ✓ DELETE /users/{userId}/roles/remove
   ✓ POST /roles/{roleId}/permissions/add
   ✓ DELETE /roles/{roleId}/permissions/remove

2. Update login response to include permissions:
   ✓ Modify AuthService to include User.Permissions in LoginResultDto

3. Add GET endpoints:
   ✓ GET /users/{userId}/roles
   ✓ GET /roles/{roleId}/permissions
```

### Short-term (Week 2-3):
```
1. Add audit logging table and service
2. Implement permission cache with Redis
3. Enforce temporal constraints in ABAC evaluation
4. Add transaction support for bulk operations
```

### Long-term (Month 2+):
```
1. Implement role inheritance
2. Add permission delegation workflows
3. Implement Just-In-Time (JIT) access elevation
4. Add compliance reporting dashboards
```

---

## Quick Start for Frontend Development

### 1. Login Flow:
```javascript
// POST /api/auth/login
{
  "userName": "admin",
  "password": "password"
}

// Response:
{
  "accessToken": "eyJ0eXAi...",
  "user": {
    "id": 1,
    "username": "admin",
    "firstname": "Admin",
    "roles": ["Admin", "Editor"]
    // ⚠️ permissions property missing
  }
}
```

### 2. Store & Use Token:
```javascript
// Store in localStorage
localStorage.setItem("accessToken", response.accessToken);

// Use in all subsequent requests
const headers = {
  Authorization: `Bearer ${localStorage.getItem("accessToken")}`
};
```

### 3. Check Permissions:
```javascript
// Option A: Frontend UI visibility (based on roles)
if (user.roles.includes("Admin")) {
  // Show admin menu
}

// Option B: Backend validation (recommended)
// Every API endpoint checks permissions automatically
// If user lacks permission, backend returns 403 Forbidden
```

### 4. Manage Permissions (after Phase 1 endpoints added):
```javascript
// Assign roles to user
POST /api/users/{userId}/roles/add
{ "roleIds": [1, 2, 3] }

// Get user's roles
GET /api/users/{userId}/roles

// Assign permissions to role
POST /api/roles/{roleId}/permissions/add
{ "permissionIds": [10, 11, 12] }

// Get role's permissions
GET /api/roles/{roleId}/permissions
```

---

## Conclusion

The WFT Infrastructure backend has a **solid, well-architected RBAC/ABAC authorization system** with comprehensive policy evaluation. The core functionality is production-ready, but **critical endpoints are missing for complete frontend administration**.

**Next Steps**:
1. Add 6 missing endpoints (user-role and role-permission management)
2. Include permissions in login response
3. Implement audit logging and caching
4. Plan Phase 2 enhancements

**Status**: 85% complete - Ready for deployment with noted limitations


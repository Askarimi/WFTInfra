# RBAC & ABAC Implementation - Executive Summary

## Quick Overview

The WFT Infrastructure backend implements a **two-tier authorization system**:
- **RBAC (Role-Based Access Control)**: Primary mechanism for permission management
- **ABAC (Attribute-Based Access Control)**: Advanced layer for fine-grained policies

---

## 1. Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER LOGIN                              │
│                      /api/auth/login                            │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                    ┌──────────▼─────────┐
                    │  Validate Creds    │
                    │ (IAuthService)     │
                    └──────────┬─────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
   ┌────▼──────┐         ┌────▼──────┐       ┌──────▼────┐
   │  GetRoles │         │GetPerms   │       │GenJWT     │
   │  (SQL)    │         │(SQL)      │       │(Token)    │
   └─────────┬─┘         └────┬──────┘       └──────┬────┘
        │                    │                      │
        └────────┬───────────┴──────────┬───────────┘
                 │                      │
           ┌─────▼──────────────────┐  │
           │  LoginResultDto        │  │
           │  - AccessToken (JWT)   │  │
           │  - RefreshToken        │  │
           │  - User{Roles[], Perms[]}
           └────────────────────────┘
                      │
                      ▼
        ┌─────────────────────────────────┐
        │ Frontend (React/Angular)        │
        │ - Stores JWT in localStorage    │
        │ - Displays UI based on perms    │
        │ - Sends JWT with each request   │
        └─────────────────────────────────┘
```

---

## 2. RBAC Permission Resolution

```
API Request
    │
    ├─ Authorization Header: Bearer {JWT}
    │
    └─▶ PermissionHandler (AuthorizationService)
         │
         ├─▶ Extract userId from JWT claims
         │
         └─▶ HasPermissionAsync(userId, permissionName)
              │
              ├─▶ IUserRepository.HasPermissionAsync()
              │   │
              │   └─▶ SQL Query:
              │       ┌─────────┐
              │       │  Users  │
              │       └────┬────┘
              │            │ (FK: UserRoles)
              │       ┌────▼────┐
              │       │UserRoles│
              │       └────┬────┘
              │            │ (FK: Roles)
              │       ┌────▼────┐
              │       │  Roles  │
              │       └────┬────┘
              │            │ (FK: RolePermissions)
              │       ┌────▼────────────┐
              │       │RolePermissions │
              │       └────┬────────────┘
              │            │ (FK: Permissions)
              │       ┌────▼──────┐
              │       │Permissions│
              │       │ WHERE      │
              │       │ IsActive=1 │
              │       └────────────┘
              │
              ├─ FOUND? Return true  ✓
              │
              └─ NOT FOUND? Return false ✗
```

---

## 3. ABAC Policy Evaluation

```
Policy Rule
├─ Name: "DepartmentDocumentAccess"
├─ Effect: "Allow"
├─ Priority: 1
│
└─ Conditions (AND/OR):
   ├─ Department = "Finance"
   ├─ DocumentStatus = "Approved"
   └─ UserLevel >= "Manager"

Evaluation Process:
┌─────────────────────────────────────────────────┐
│ 1. Check RBAC (baseline)                        │
│    ├─ User has permission? No ──▶ DENY         │
│    └─ User has permission? Yes ──▶ Continue    │
│                                                 │
│ 2. Get User's ABAC Policies                    │
│    ├─ User's Roles: [Manager, Accountant]      │
│    └─ Policies:                                │
│       ├─ DepartmentDocumentAccess (Active)    │
│       ├─ SecureDataAccess (Denied)            │
│       └─ AuditReportAccess (Active)           │
│                                                 │
│ 3. Sort by Priority (1 = first)               │
│    └─ Evaluate in order:                       │
│       1️⃣ DepartmentDocumentAccess             │
│       2️⃣ AuditReportAccess                    │
│                                                 │
│ 4. Evaluate Conditions for Each Policy        │
│    DepartmentDocumentAccess:                  │
│    ├─ Ali's Department = Finance ✓            │
│    ├─ Document Status = Approved ✓            │
│    ├─ Ali's Level = Senior ≥ Manager ✓       │
│    ├─ All conditions met ✓                    │
│    ├─ Effect = Allow ✓                        │
│    └─ GRANT ACCESS ✓                          │
│                                                 │
│ 5. Default: If no policy matches → DENY      │
└─────────────────────────────────────────────────┘
```

---

## 4. Database Schema

### Core RBAC Tables

```
┌──────────┐         ┌──────────┐         ┌─────────────┐
│  Users   │         │  Roles   │         │Permissions │
├──────────┤         ├──────────┤         ├─────────────┤
│Id (PK)   │         │Id (PK)   │         │Id (PK)      │
│Username  │         │Name (U)  │         │DisplayName  │
│Email     │         │IsActive  │         │IsActive     │
│IsActive  │         │          │         │IsActive     │
└────┬─────┘         └────┬─────┘         └─────┬───────┘
     │                    │                     │
     │◄──────────────────►│◄────────────────────►
        UserRoles      RolePermissions
        (Join table)     (Join table)
        UserId(FK)       RoleId(FK)
        RoleId(FK)       PermissionId(FK)
```

### ABAC Tables

```
┌──────────────┐         ┌─────────────────┐         ┌────────────┐
│  PolicyRules │         │PolicyConditions │         │Attributes │
├──────────────┤         ├─────────────────┤         ├────────────┤
│Id (PK)       │         │Id (PK)          │         │Id (PK)     │
│Name          │◄────────│PolicyRuleId(FK) │         │Name        │
│Effect (A/D)  │         │                 │    ┌───►│DataType    │
│Priority      │         │AttributeId(FK)  ├────┤    │Source      │
│IsActive      │         │Operator         │    │    │IsActive    │
└──────┬───────┘         │Value            │    │    └────────────┘
       │                 │LogicalOp (A/O)  │    │
       │                 └─────────────────┘    │
       │                                        │
       └───────────────────────────────────────┘
            RolePolicyRules
            (Join table)
            RoleId(FK)
            PolicyRuleId(FK)
```

---

## 5. API Endpoints Checklist

### ✓ Implemented Endpoints

```
Authentication
├─ POST   /api/auth/login          (Login & get JWT)
├─ POST   /api/auth/register       (Create account)
└─ POST   /api/auth/logout         (Revoke token)

Users
├─ POST   /api/users/Add           (Create user)
├─ GET    /api/users/init/{id}     (Get user)
├─ GET    /api/users/List          (List users)
├─ PUT    /api/users/update        (Update user)
└─ DELETE /api/users/delete/{id}   (Delete user)

Roles
├─ POST   /api/roles/Add           (Create role)
├─ GET    /api/roles/init/{id}     (Get role)
├─ GET    /api/roles/List          (List roles)
├─ PUT    /api/roles/update        (Update role)
└─ DELETE /api/roles/delete/{id}   (Delete role)

Permissions
├─ POST   /api/permissions/Add     (Create permission)
├─ GET    /api/permissions/init/{id} (Get permission)
├─ GET    /api/permissions/List    (List permissions)
├─ PUT    /api/permissions/update  (Update permission)
└─ DELETE /api/permissions/delete/{id} (Delete permission)

ABAC Policies
├─ POST   /api/policyrules/Add     (Create policy)
├─ GET    /api/policyrules/List    (List policies)
├─ PUT    /api/policyrules/update  (Update policy)
├─ DELETE /api/policyrules/delete  (Delete policy)
└─ POST   /api/abac/evaluate       (Test policy)

Policy-Role Assignment
├─ POST   /api/rolepolicyassignment/assign/{roleId}/{policyId}
├─ DELETE /api/rolepolicyassignment/remove/{roleId}/{policyId}
└─ GET    /api/rolepolicyassignment/role/{roleId}
```

### ❌ Missing Critical Endpoints

```
Role-Permission Assignment (MISSING)
├─ POST   /api/rolepermission/assign/{roleId}/{permissionId}
├─ DELETE /api/rolepermission/remove/{roleId}/{permissionId}
├─ GET    /api/rolepermission/role/{roleId}
└─ POST   /api/rolepermission/bulkassign/{roleId}

User-Role Assignment (MISSING)
├─ POST   /api/userrole/assign/{userId}/{roleId}
├─ DELETE /api/userrole/remove/{userId}/{roleId}
├─ GET    /api/userrole/user/{userId}
└─ POST   /api/userrole/bulkassign/{userId}

Policy Conditions (MISSING)
├─ POST   /api/policyconditions/add
├─ PUT    /api/policyconditions/update/{id}
└─ DELETE /api/policyconditions/delete/{id}

Permission Details (MISSING)
└─ GET    /api/users/{userId}/permissions (Detailed metadata)
```

---

## 6. Data Flow: User → Roles → Permissions

```
User "Ali"
├─ ID: 5
│
└─▶ UserRole Entries:
   ├─ (UserId=5, RoleId=2)
   │  └─▶ Role "Manager"
   │      └─▶ RolePermission:
   │          ├─ PermissionId=10 → "EditUser"
   │          ├─ PermissionId=11 → "ViewReports"
   │          └─ PermissionId=15 → "CreateReport"
   │
   └─ (UserId=5, RoleId=3)
      └─▶ Role "Accountant"
          └─▶ RolePermission:
              ├─ PermissionId=11 → "ViewReports"
              ├─ PermissionId=20 → "ApproveExpense"
              └─ PermissionId=21 → "GenerateLedger"

Result: Ali's Permissions = [EditUser, ViewReports, CreateReport, ApproveExpense, GenerateLedger]
         (Duplicates removed by .Distinct())
```

---

## 7. Login Response Example

```json
POST /api/auth/login
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64EncodedLongString==",
    "user": {
      "id": 5,
      "username": "ali",
      "firstName": "Ali",
      "lastName": "Mohammadi",
      "email": "ali@example.com",
      "isActive": true,
      "lastLoginAt": "2025-10-24T12:00:00Z",
      "roles": ["Manager", "Accountant"],
      "permissions": ["EditUser", "ViewReports", "CreateReport", "ApproveExpense", "GenerateLedger"]
    }
  },
  "isSuccess": true,
  "message": null,
  "statusCode": 200
}
```

**Note**: Permissions are strings only. No metadata (displayName, description) provided.

---

## 8. JWT Token Claims

```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "5",                    // User ID (subject)
  "unique_name": "ali",          // Username
  "role": ["Manager", "Accountant"],  // Role claims (multiple)
  "permission": [                     // Permission claims (multiple)
    "EditUser",
    "ViewReports",
    "CreateReport",
    "ApproveExpense",
    "GenerateLedger"
  ],
  "iat": 1697987654,            // Issued at
  "exp": 1697991254,            // Expires in
  "iss": "WFT.Infra",           // Issuer
  "aud": "WFT.Infra.Users"      // Audience
}

Signature:
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  secret
)
```

---

## 9. Critical Gaps Summary

| Gap | Impact | Priority | Fix |
|-----|--------|----------|-----|
| No Role-Permission Assignment API | Cannot manage role perms via UI | 🔴 Critical | Create controller |
| No User-Role Assignment API | Cannot assign roles to users | 🔴 Critical | Create controller |
| Permission metadata missing | No localized descriptions in UI | 🟠 High | Extend DTO |
| No real-time permission sync | Must re-login for updates | 🟠 High | Add refresh endpoint |
| No granular policy condition API | Must create entire policy at once | 🟡 Medium | Add CRUD endpoints |

---

## 10. Security Posture

| Aspect | Status | Notes |
|--------|--------|-------|
| RBAC Validation | ✓ Strong | Checked on every endpoint |
| ABAC Policies | ✓ Strong | Fine-grained attribute checks |
| Token Security | ✓ Good | JWT in memory, RefreshToken in HttpOnly cookie |
| Default Deny | ✓ Correct | ABAC defaults to deny if no policy matches |
| Permission Normalization | ✓ Good | Case-insensitive, trims whitespace |
| Database Constraints | ✓ Excellent | Composite unique indexes prevent duplicates |

---

## 11. Recommended Implementation Order

### Phase 1: Enable Dynamic Management (Week 1)
- [ ] Create `RolePermissionAssignmentController`
- [ ] Create `UserRoleAssignmentController`
- [ ] Write unit tests for new endpoints
- [ ] Update frontend UI for role/user management

### Phase 2: Enhance UX (Week 2)
- [ ] Extend `LoginResultDto` with detailed permission objects
- [ ] Create `/users/{id}/permissions` endpoint
- [ ] Implement permission refresh endpoint
- [ ] Add real-time permission updates

### Phase 3: Optimization (Week 3)
- [ ] Implement Redis caching for user permissions
- [ ] Batch ABAC policy queries
- [ ] Add audit logging for permission changes
- [ ] Create ABAC policy templates

---

## 12. Conclusion

✅ **Strengths**:
- Solid RBAC foundation
- Comprehensive ABAC framework
- Clean layered architecture
- Proper database design with indexes

❌ **Weaknesses**:
- Incomplete REST API for role/permission management
- Limited login response metadata
- No real-time permission synchronization

🎯 **Next Step**: Implement Phase 1 to unblock frontend-driven authorization management.

---

**Document Version**: 1.0  
**Last Updated**: October 24, 2025  
**Status**: Analysis Complete, Implementation Plan Ready



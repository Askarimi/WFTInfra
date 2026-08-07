
---

### `Frontend_Summary.md`
```md
# Frontend Summary

## 1. Frontend Overview

This frontend is a React 19 application written in TypeScript and built with Vite.

It serves as an administration UI for:

- Authentication and session handling
- User management
- Role management
- Permission management
- RBAC workflows
- ABAC workflows
- Administration and configuration tasks

The UI is designed to be:

- RTL-friendly
- Persian-first
- Type-safe
- Modular
- Based on reusable service abstractions

---

## 2. Core Technologies

The frontend uses:

- React 19.1
- TypeScript 5.8
- Vite 6
- Mantine 6
- Redux Toolkit
- React Router 7
- Axios 1.9+

The project does not use:

- Native `fetch`
- React Query
- SWR
- Apollo
- RTK Query

API communication is intentionally handled through imperative service abstractions.

---

## 3. Frontend Architecture

The frontend is organized around three important concerns:

- UI components
- State management
- HTTP/service abstractions

Important infrastructure modules include:

- `src/providers/HttpProvider/apiClient.ts`
- `src/providers/HttpProvider/httpService.ts`
- `src/providers/HttpProvider/tokenManager.ts`

Domain service modules exist for:

- Authentication
- Users
- Roles
- Permissions
- Role-permission relationships
- ABAC-related features

---

## 4. API Communication

The frontend communicates with the backend through Axios.

### 4.1 Request Style

API calls are made imperatively through service functions.

Typical flow:
```text
React component
 → Domain service
 → HttpService
 → Axios
 → Vite proxy
 → ASP.NET Core controller
```

---

## 5. Authorization Change - SuperAdmin PermissionGuard Behavior

### 5.1 Change Type

- Frontend authorization and presentation guard
- Authentication state normalization
- Permission-checking behavior

### 5.2 Reason for Change

The `SuperAdmin` user could be authorized by the backend but still have actions hidden by `PermissionGuard` when the login response did not contain every individual permission. The frontend needed to recognize the system administrator role consistently so the UI would not incorrectly hide permitted administration actions.

### 5.3 Affected Files and Components

- `src/utils/permissionChecker.ts`
  - `hasPermission` now treats `SuperAdmin` as having every permission.
  - `hasAnyPermission` and `hasAllPermissions` inherit the bypass through `hasPermission`.
  - Resource/action helper methods used by guards inherit the same behavior.
- `src/components/common/PermissionGuard.tsx`
  - Continues to render through the centralized `PermissionChecker`.
  - Supports permission arrays, a single permission, and legacy resource/action checks.
- `src/features/auth/store/authSlice.ts`
  - Loads roles from `loginData.user.roles`, ensuring the current user contains the role data used by `PermissionChecker`.

### 5.4 Behavior Before and After

**Before:**

- Direct permission checks depended only on the user's explicit permission array.
- A `SuperAdmin` without individual permission entries could have guarded buttons or actions hidden.
- Role hydration used the outer login result role collection, which could leave the current user's role state inconsistent with the returned user object.

**After:**

- `PermissionChecker.hasPermission` returns `true` when the current user has the `SuperAdmin` role.
- `PermissionGuard` displays protected UI through its single-permission, multiple-permission, and resource/action paths when the SuperAdmin role satisfies the underlying checker.
- Authentication state reads roles from the login response's user object, allowing the bypass to operate on the normalized current-user state.
- Non-SuperAdmin users continue to require their explicit permissions or the existing role-based fallback.

### 5.5 Verification

Verify the frontend behavior with these scenarios:

1. Sign in as a user whose `roles` contains `SuperAdmin` and whose `permissions` is empty.
2. Confirm that UI elements protected by `PermissionGuard` are visible for:
   - a single `permission`
   - a `permissions` array
   - a legacy `action` and `resource` pair
3. Confirm that the related backend operations succeed.
4. Sign in as a normal user without the required permission and confirm the same elements remain hidden or render their fallback.
5. Refresh or restore the session and confirm the normalized user state still contains the `SuperAdmin` role.

### 5.6 Security Notes

- `PermissionGuard` is a UX and presentation control only; users can bypass client-side code.
- Every protected operation must still be authorized by the backend `AuthorizationService`.
- Role values come from authenticated session data and must not be trusted as independent client-side proof of authorization.
- The frontend and backend must use the same canonical `SuperAdmin` role name.
- Assignment of the SuperAdmin role must be restricted and audited because it intentionally grants unrestricted authorization.

---

## 6. API and ABAC Integration Changes

### 6.1 Change Type

- HTTP response contract alignment
- Pagination normalization
- ABAC policy-rule list integration
- Administration UI consistency

### 6.2 Affected Files and Behavior

- `src/providers/HttpProvider/httpService.ts`
  - `getPaged` now reads list data from the top-level API `data` array and pagination values from the top-level `meta` object.
  - Missing data and metadata receive safe empty/default values.
- `src/services/abac.service.ts`
  - Policy-rule listing now uses the shared `getPaged` flow and passes `PageNumber`, `PageSize`, and `SearchTerm`.
- `src/types/abac.ts`
  - Adds `PolicyRuleQueryParameters` for typed policy-rule list queries.
- `src/features/abacManagement/PolicyRuleList.tsx`
  - Aligns list loading, paging, searching, editing, deletion, and refresh behavior with the shared paged response.
- `src/features/abacManagement/abacColumns.tsx`
  - Aligns the policy-rule table columns and action rendering with the current administration-table behavior.

### 6.3 Verification

- Load the policy-rule list and confirm rows are read from `data`.
- Change page number and page size and confirm `meta.totalCount` and `meta.totalPages` drive the table correctly.
- Search, create, edit, and delete a policy rule and confirm the list refreshes.
- Confirm failed or empty responses produce an empty list instead of an undefined-data error.

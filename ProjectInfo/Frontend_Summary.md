
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

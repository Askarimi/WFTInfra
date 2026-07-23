# WFT.Infra.UI React Frontend Workspace Summary

This document summarizes the frontend workspace for use by an external AI assistant. The application is a Persian, right-to-left administration UI focused on authentication, user/role/permission management, and attribute-based access control (ABAC).

## 1. Tech Stack & Core Tools

### Runtime and Language

- React: `^19.1.0`
- React DOM: `^19.1.0`
- TypeScript: `^5.8.3`
- Source format: TypeScript and TSX (`.ts` and `.tsx`)
- Module system: ECMAScript modules (`"type": "module"` in `package.json`)
- JSX transform: `react-jsx`
- Browser compilation target: ES2020
- Node/config compilation target: ES2022
- TypeScript strict mode is enabled.

### Build Tool and Bundling

- Build tool/dev server: Vite `^6.3.5`
- React integration: `@vitejs/plugin-react` `^4.4.1`
- Vite uses Rollup for production bundling.
- Development server:
  - Host: `127.0.0.1`
  - Port: `3000`
  - Proxies `/api/app/v1/wft` to `https://localhost:7041`
  - Proxy TLS verification is disabled for the local backend (`secure: false`).
- Production build command: `tsc -b && vite build`
- Build output directory: Vite default `dist/`

### Package Manager

- Package manager: Yarn Classic
- Declared version: `yarn@1.22.19`
- Lockfile: `yarn.lock`
- No npm or pnpm lockfile is present.

### Development Tooling

- ESLint: `^9.26.0`, using flat configuration in `eslint.config.js`
- TypeScript ESLint: `typescript-eslint` and `@typescript-eslint/*`
- React lint plugins:
  - `eslint-plugin-react-hooks`
  - `eslint-plugin-react-refresh`
- Prettier: `^3.5.3` is installed, but no dedicated Prettier configuration or formatting script is present.
- React Strict Mode wraps the application in `src/main.tsx`.

### Package Scripts

```text
yarn dev      -> vite
yarn build    -> tsc -b && vite build
yarn lint     -> eslint .
yarn preview  -> vite preview
```

## 2. State Management & Routing

### State Management

- Global state uses Redux Toolkit `^2.8.2`.
- React bindings use React Redux `^9.2.0`.
- The Redux store is defined in `src/features/auth/store/index.ts`.
- The store currently contains one reducer:
  - `auth`: authentication user, token, loading/error state, authentication status, and rehydration status.
- Typed Redux hooks are defined in `src/hooks.ts`:
  - `useAppDispatch`
  - `useAppSelector`
- Authentication async actions use Redux Toolkit `createAsyncThunk`:
  - `login`
  - `logoutUser`
- Authentication is persisted manually in browser `localStorage`:
  - `access_token`
  - `user`
- `rehydrateAuth()` reads authentication data from `localStorage` when `App` mounts.
- Theme state is not stored in Redux. A custom `useLocalStorage` hook persists the `theme` value.
- Component-local state uses standard React hooks such as `useState`, `useEffect`, and `useMemo`.
- There is no Zustand, MobX, Recoil, Jotai, XState, Redux Persist, or React Context-based application state layer.

### Routing

- Routing library: `react-router-dom` `^7.6.0`
- The code uses the declarative `BrowserRouter`, `Routes`, `Route`, `Navigate`, and `Outlet` API familiar from React Router v6.
- Navigation/layout code also uses `useNavigate` and `useLocation`.
- `PrivateRoute` guards authenticated pages:
  - Shows a loading message while authentication is rehydrating.
  - Renders an `Outlet` for authenticated users.
  - Redirects unauthenticated users to `/login`.
- `AppShellLayout` is a nested layout route that renders navigation plus the current page through another `Outlet`.

### Route Map

Public routes:

```text
/                 -> redirects to /login
/login            -> LoginPage
```

Authenticated routes inside `PrivateRoute` and `AppShellLayout`:

```text
/dashboard         -> Dashboard
/users             -> UserList
/permissions       -> PermissionList
/roles             -> RoleList
/attributes        -> AttributeDefinitionList
/policies          -> PolicyRuleList
/operators         -> ConditionOperatorList
/assignments       -> RolePolicyAssignmentList
/evaluation        -> AccessEvaluationPanel
/user-attributes   -> UserAttributeList
/attribute-groups  -> AttributeGroupList
/settings          -> placeholder ("coming soon")
```

### Authorization Model

- UI authorization supports both role-based access control (RBAC) and ABAC.
- `PermissionChecker` reads the authenticated user from Redux and exposes checks for:
  - Roles and admin roles
  - Individual permission names
  - Any/all permission checks
  - User management operations
  - Backend ABAC evaluation for operations on specific users
- `PermissionGuard` conditionally renders UI based on one permission, multiple permissions, or resource/action combinations.
- ABAC checks call the backend `/abac/evaluate` endpoint.

## 3. Styling & UI Components

### Primary UI Framework

- Primary component library: Mantine v6 (`@mantine/core` `6.0.21`)
- Mantine packages declared in `package.json` include:
  - Core
  - Forms
  - Modals
  - Notifications
  - Hooks
  - Dates
  - Dropzone
  - Carousel
  - NProgress
  - Prism
  - Spotlight
  - Tiptap integration
- Commonly observed Mantine usage:
  - `MantineProvider`
  - `ColorSchemeProvider`
  - `AppShell`, `Navbar`, `Header`, `NavLink`, `Burger`
  - Form controls, buttons, modals, notifications, layout primitives, and panels
- Form state/validation is handled through `@mantine/form`.
- Modal orchestration uses `@mantine/modals`.
- Toast/status feedback uses `@mantine/notifications`.

### Direction, Theme, and Fonts

- The interface is designed for Persian/Farsi content and RTL layout.
- `index.html` sets `<html dir="rtl">`.
- Mantine theme configuration sets `dir: "rtl"`.
- Emotion RTL transformation uses:
  - `@emotion/react` `11`
  - `stylis-plugin-rtl` `^2.1.1`
  - A custom Mantine Emotion cache created in `src/App.tsx`
- Light/dark theme selection is saved to `localStorage` under `theme`.
- The main font is Vazirmatn.
- Local font assets include Vazirmatn and Shabnam formats.
- `index.html` also requests Vazirmatn from Google Fonts in addition to defining a local `@font-face`.

### Tables and Icons

- Data-table UI uses `mantine-react-table` `^1.0.0`.
- `src/components/DataTable/DataTable.tsx` provides a reusable generic table with:
  - Server-controlled pagination
  - Server-controlled global filtering/search
  - Row selection
  - Loading state
  - Persian localization
- Feature-specific column definitions use `MRT_ColumnDef`.
- `@tanstack/react-table` `^8.21.3` is declared because Mantine React Table builds on the TanStack table ecosystem, but application source does not import it directly.
- Legacy `react-table` `^7.8.0` and `@types/react-table` are declared but are not imported by current source files.
- Icons use `@tabler/icons-react` `^3.33.0`.

### Rich Text and Other UI Dependencies

- Tiptap packages are declared:
  - `@tiptap/react`
  - `@tiptap/starter-kit`
  - `@tiptap/extension-link`
- `dayjs` and `embla-carousel-react` are also declared.
- Current `src` code does not appear to import Tiptap, Day.js, Embla, or most optional Mantine subpackages.

### Styling Approach

- Styling is primarily Mantine props, theme values, component `styles`, and inline `style` objects.
- There are no CSS, Sass, Less, or Stylus files under `src` or `public`.
- Tailwind CSS is not installed or configured.
- Material UI/MUI is not installed or used.
- Styled Components and CSS Modules are not used.

## 4. API Communication

### HTTP Client

- API communication uses Axios `^1.9.0`.
- There is no use of the native `fetch()` API.
- React Query/TanStack Query is not installed or used.
- SWR, Apollo Client, and RTK Query are not used.
- API calls are invoked imperatively from feature components/services and stored in local component state where needed.

### API Base URL and Development Proxy

- `.env` defines:

```text
VITE_API_BASE_URL= /api/app/v1/wft
```

- `src/providers/HttpProvider/apiClient.ts` reads `import.meta.env.VITE_API_BASE_URL`.
- Vite proxies that path to the local HTTPS backend at `https://localhost:7041`.
- The Axios instance sends JSON and enables `withCredentials: true`.

### HTTP Abstraction

The HTTP layer is split into:

```text
src/providers/HttpProvider/apiClient.ts
  Axios instance, base URL, auth header, response interceptor, token refresh

src/providers/HttpProvider/httpService.ts
  Typed get/post/put/delete helpers, API envelope handling, paged-query helper

src/providers/HttpProvider/tokenManager.ts
  Present but currently empty
```

`HttpService` expects a C#-style response envelope resembling:

```text
{
  success: boolean,
  data: T,
  message?: string,
  error?: string,
  logId?: string,
  meta?: pagination metadata
}
```

`getPaged()` normalizes backend pagination into a structure containing `data`, `meta`, `success`, `message`, and legacy pagination properties such as `totalCount`.

### Authentication and Token Refresh

- A request interceptor reads `access_token` from `localStorage` and sends it as `Authorization: Bearer <token>`.
- A response interceptor handles HTTP 401 responses.
- Only one refresh request is allowed at a time.
- Concurrent failed requests are queued until the refresh completes.
- Refresh endpoint: `/auth/refresh-token`
- On refresh success:
  - The new access token is stored.
  - Queued/original requests are retried.
- On refresh failure:
  - The access token is removed.
  - The browser is redirected to `/login`.
- Login, refresh, and logout functions live in `src/features/auth/services/authApi.ts`.

### Domain Service Modules

```text
src/services/user.service.ts
  User listing, initialization, create, update, delete, password setting

src/services/role.service.ts
  Role listing, initialization, create, update, delete

src/services/permission.service.ts
  Permission listing, initialization, create, update, delete

src/services/rolePermission.service.ts
  Assign permissions to a role and retrieve role permissions

src/services/abac.service.ts
  Attribute definitions, attribute groups, policy rules, condition operators,
  role-policy assignments, user attributes, and access evaluation
```

List pages use backend-driven paging and search through `PagedQueryParams` and `PagedResponse` types.

### API Implementation Caveat

- `HttpService.get`, `put`, and `delete` generally return the inner `data` value.
- `HttpService.post` currently returns the response envelope when `response.success` is true rather than consistently returning `response.data`.
- Some callers compensate for this behavior, but any API refactor should first standardize the response-unwrapping contract and then update all callers together.

## 5. Project Structure Tree

```text
src/
├── App.tsx
│   └── Root providers, RTL/theme setup, authentication rehydration, route map
├── main.tsx
│   └── React entry point and Redux Provider
├── hooks.ts
│   └── Typed Redux hooks, localStorage hook, clock hook
├── makeData.ts
│   └── Sample/mock data helper
├── vite-env.d.ts
│
├── assets/
│   ├── fonts/
│   │   ├── Shabnam.eot
│   │   ├── Shabnam.ttf
│   │   ├── Shabnam.woff
│   │   ├── Shabnam.woff2
│   │   └── Vazirmatn-Regular.woff2
│   ├── images/
│   │   └── 5153829.jpg
│   └── react.svg
│
├── components/
│   ├── PrivateRoute.tsx
│   ├── DataTable/
│   │   ├── Columns.ts
│   │   ├── DataTable.tsx
│   │   ├── localizationfa.ts
│   │   └── useDataTable.ts
│   ├── common/
│   │   ├── ActionButtons.tsx
│   │   ├── PermissionGuard.tsx
│   │   ├── WFTFormContainer.tsx
│   │   ├── WFTFormWrapper.tsx
│   │   └── WFTPanel.tsx
│   └── layout/
│       ├── AppShellLayout.tsx
│       ├── AvatarInfo.tsx
│       ├── Header.tsx
│       ├── LinksGroup.tsx
│       ├── LogoutButton.tsx
│       └── Sidebar.tsx
│
├── features/
│   ├── auth/
│   │   ├── components/
│   │   │   └── LoginForm.tsx
│   │   ├── pages/
│   │   │   └── LoginPage.tsx
│   │   ├── services/
│   │   │   └── authApi.ts
│   │   ├── store/
│   │   │   ├── authSlice.ts
│   │   │   └── index.ts
│   │   └── types/
│   │       └── auth.ts
│   │
│   ├── userManagment/
│   │   ├── UserEditor.tsx
│   │   ├── UserList.tsx
│   │   ├── UserPasswordChangeModal.tsx
│   │   ├── UserPasswordEditor.tsx
│   │   └── userColumns.tsx
│   │
│   ├── roleManagment/
│   │   ├── RoleDetailModal.tsx
│   │   ├── RoleEditor.tsx
│   │   ├── RoleList.tsx
│   │   ├── index.ts
│   │   └── roleColumns.tsx
│   │
│   ├── permissionManagment/
│   │   ├── PermissionEditor.tsx
│   │   ├── PermissionList.tsx
│   │   ├── index.ts
│   │   └── permissionColumns.tsx
│   │
│   └── abacManagement/
│       ├── AccessEvaluationPanel.tsx
│       ├── AttributeDefinitionEditor.tsx
│       ├── AttributeDefinitionList.tsx
│       ├── AttributeGroupEditor.tsx
│       ├── AttributeGroupList.tsx
│       ├── ConditionOperatorEditor.tsx
│       ├── ConditionOperatorList.tsx
│       ├── PolicyRuleEditor.tsx
│       ├── PolicyRuleList.tsx
│       ├── RolePolicyAssignmentEditor.tsx
│       ├── RolePolicyAssignmentList.tsx
│       ├── UserAttributeEditor.tsx
│       ├── UserAttributeList.tsx
│       ├── abacColumns.tsx
│       └── index.ts
│
├── pages/
│   └── Dashboard.tsx
│
├── providers/
│   └── HttpProvider/
│       ├── apiClient.ts
│       ├── httpService.ts
│       └── tokenManager.ts
│
├── services/
│   ├── abac.service.ts
│   ├── permission.service.ts
│   ├── role.service.ts
│   ├── rolePermission.service.ts
│   └── user.service.ts
│
├── types/
│   ├── abac.ts
│   ├── pagedQueryParams.ts
│   ├── permission.ts
│   ├── role.ts
│   └── user.ts
│
└── utils/
    ├── permissionChecker.ts
    └── permissionUtils.ts
```

### Structural Notes

- The intended architecture is feature-based and domain-oriented.
- Authentication is fully grouped under `features/auth`.
- Other domain features contain UI components locally, while their service and type modules remain in shared top-level `services/` and `types/` directories.
- Shared UI is separated into data-table, common, and layout components.
- Some filenames use the misspelling `Managment` instead of `Management`; imports must preserve the existing names unless the folders are deliberately renamed everywhere.
- `Columns.ts`, `useDataTable.ts`, and `tokenManager.ts` are currently empty placeholder files.
- Several layout components coexist with `AppShellLayout`; current route rendering is centered on `AppShellLayout`.

## 6. Key Configuration Files

### `package.json`

- Defines dependencies, dev dependencies, scripts, ESM mode, and Yarn version.
- Important version relationships:
  - React 19
  - Vite 6
  - TypeScript 5.8
  - Mantine 6
  - React Router DOM 7
  - Redux Toolkit 2
  - Axios 1

### `vite.config.ts`

- Registers `@vitejs/plugin-react`.
- Runs the local dev server on `127.0.0.1:3000`.
- Proxies the application API prefix to `https://localhost:7041`.
- Accepts the backend's local/self-signed HTTPS certificate.

### `.env`

- Sets `VITE_API_BASE_URL` to `/api/app/v1/wft`.
- The value currently contains whitespace after `=`, so consumers should verify the parsed environment value if base-URL behavior is unexpected.
- Only variables prefixed with `VITE_` are exposed to client-side Vite code.

### `tsconfig.json`

- Acts as the TypeScript solution file.
- References `tsconfig.app.json` and `tsconfig.node.json`.

### `tsconfig.app.json`

- Applies to `src`.
- Uses strict checking and no emit.
- Uses bundler module resolution and the React JSX transform.
- Enables unused-variable/parameter checks, fallthrough checks, and side-effect import checks.

### `tsconfig.node.json`

- Applies to `vite.config.ts`.
- Uses ES2022/ES2023 settings, strict checking, bundler resolution, and no emit.

### `eslint.config.js`

- ESLint flat configuration.
- Lints TypeScript and TSX files.
- Extends JavaScript and TypeScript recommended rules.
- Enables React Hooks rules and the React Refresh component-export warning.
- Ignores `dist`.
- It does not currently configure type-aware TypeScript linting.

### `index.html`

- Defines the root DOM element (`#root`) and loads `/src/main.tsx`.
- Sets RTL document direction.
- Defines/loads the Vazirmatn font.
- Sets the page title to `WFT-Infra`.

### `yarn.lock`

- Locks the Yarn dependency graph.
- Should be updated through Yarn rather than npm/pnpm.

### `.gitignore`

- Controls ignored local/build artifacts.
- The existing `dist/` directory is a generated Vite build artifact and should not be treated as source.

### `src/vite-env.d.ts`

- Provides Vite client-side TypeScript environment declarations.

## Additional Notes for an External AI Assistant

- Primary language in UI labels, validation messages, comments, and notifications is Persian.
- Preserve RTL behavior when modifying layout, spacing, icons, forms, tables, modals, or notifications.
- Preserve the feature-oriented organization and reuse shared `DataTable`, form wrappers, panels, permission guards, and service abstractions.
- Route access authentication and action-level authorization are separate concerns:
  - `PrivateRoute` handles authenticated access.
  - `PermissionGuard`/`PermissionChecker` handle role and permission checks.
- Authentication relies on both cookies (`withCredentials`) and a bearer access token; refresh behavior depends on the backend setting/accepting credentials correctly.
- List screens use server-side pagination/search rather than downloading all rows.
- There is no automated test framework or test script declared in `package.json`.
- There are no path aliases configured; imports are relative.
- No Tailwind, MUI, React Query, CSS framework, or CSS preprocessor should be assumed.
- `@types/react-router-dom` `^5.3.3` is declared even though modern React Router packages include their own typings; this is a potential stale dependency.
- Before large dependency upgrades, verify compatibility among React 19, Mantine 6, Mantine React Table 1, and React Router 7.

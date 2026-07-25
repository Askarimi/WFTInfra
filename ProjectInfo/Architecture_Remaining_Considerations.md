# Architecture Remaining Considerations

## Status

The previously identified `HttpService` response unwrapping inconsistency has been reviewed and is currently considered **resolved/stabilized** based on the latest frontend service and authentication-layer alignment.

This document records the **remaining architecture considerations** that should be preserved in project documentation and reviewed as the project evolves.

---

## 1) JWT Refresh Contract Dependency

### Summary

The JWT refresh flow depends on the **exact backend refresh endpoint** and the **precise response shape** returned by that endpoint.

### Risk

If the backend refresh contract changes without coordinated frontend updates, the authentication refresh flow may fail silently or break session continuity.

### Required Rule

Any change to:

- refresh token endpoint path
- request DTO
- response DTO
- response envelope shape
- token payload structure

must be reflected in:

- frontend authentication service
- refresh/retry interceptor logic
- auth state restoration behavior

### Recommended Action

- Keep refresh endpoint contract explicitly documented
- Validate refresh response assumptions in frontend auth flow
- Re-test token refresh whenever auth contract changes

---

## 2) Backend Authorization Must Remain the Source of Truth

### Summary

Frontend permission guards improve UX and route visibility, but they must never be treated as the primary authorization mechanism.

### Risk

If the system relies on UI hiding, route guards, or client-side permission checks alone, unauthorized operations may still be attempted directly against backend endpoints.

### Required Rule

Backend controllers, handlers, and application services must independently reject unauthorized operations even when:

- the UI hides actions
- a route is protected in frontend
- a button is not rendered
- the client claims a permission state

### Recommended Action

- Treat frontend guards as presentation-level protection only
- Enforce authorization in backend endpoints and application layer
- Keep permission-sensitive business rules server-side

---

## 3) Backend/Frontend Contract Synchronization

### Summary

Changes in backend contracts must always be synchronized with frontend TypeScript models and domain services.

### Risk

Any drift between backend DTOs and frontend types may cause:

- runtime mismatches
- invalid assumptions in service logic
- broken forms, tables, or detail pages
- pagination and metadata handling errors

### Required Rule

Any change to backend:

- DTO names
- property names
- pagination metadata
- response envelope structure
- nullability rules
- enum/string conventions

must be reflected in:

- frontend interfaces/types
- service return types
- UI consumption logic
- pagination models

### Recommended Action

- Review backend/frontend contract changes together
- Update `httpService.ts` only when envelope-level behavior changes
- Update domain services and UI models when payload shape changes

---

## 4) Package / Framework Version Alignment

### Summary

Framework target versions and package major versions must be reviewed in a coordinated manner.

### Risk

Mixed or misaligned package versions may introduce:

- compatibility issues
- runtime instability
- unexpected build warnings/errors
- hidden behavioral differences across Microsoft stack components

### Required Rule

Package upgrades, especially for core Microsoft packages, should not be applied as isolated ad hoc changes.

### Recommended Action

- Review package versions as a coordinated technical task
- Validate package compatibility with the project target framework
- Execute upgrades in grouped and testable batches
- Record version alignment decisions in technical documentation

---

## 5) Security and Operational Configuration Constraints

### Summary

Some project concerns are not feature bugs but must remain visible as architectural constraints.

### Areas of Concern

#### A. Frontend Secret Exposure

Secrets must never be stored in frontend environment files if they can become part of the client bundle.

**Rule:**

- frontend environment variables are not a secure place for backend secrets
- JWT signing keys, private credentials, and sensitive internal configuration must remain server-side only

#### B. Startup / Migration / Database Dependency

If migrations, seeding, or database initialization are executed automatically during startup, the application startup path becomes dependent on database availability and permissions.

**Rule:**

- deployment environments must be prepared for DB connectivity at startup
- automatic migration behavior must be intentional and documented
- database permissions must match migration/seed strategy

### Recommended Action

- Keep operational assumptions documented
- Validate startup strategy per environment
- Separate security-sensitive configuration from client-visible settings

---

## Final Documentation Note

At the current stage of the project:

- `HttpService` standardization is considered **stabilized**
- the remaining items are primarily **architecture governance, security, contract-alignment, and operational consistency concerns**
- these items should be tracked as **ongoing technical safeguards**, not necessarily as immediate defects

---

## Suggested Next Step

After preserving these considerations in project documentation, the next recommended implementation track is:

**Designing and implementing a standardized, extensible Workflow Engine**  
based on the existing business model and aligned with the current stabilized backend/frontend architecture.

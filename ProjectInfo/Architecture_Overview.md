# Architecture Overview

## 1. System Summary

This project is a full-stack administration platform built with:

- Backend: .NET 10 ASP.NET Core Web API
- Frontend: React 19, TypeScript, Vite, Mantine, Redux Toolkit, React Router, Axios
- Database: Microsoft SQL Server
- Authentication: JWT access token and refresh token
- Architecture style: Clean Architecture with layered separation
- UI direction: RTL and Persian-first user interface

The system supports:

- Authentication and session management
- User, role, and permission management
- RBAC and ABAC authorization
- Policy, attribute, and condition management
- Pagination and filtering on the backend
- API integration through typed service abstractions on the frontend

---

## 2. High-Level Flow
```text
React Administration UI
│ HTTPS / JSON / JWT
│ /api/app/v1/wft
ASP.NET Core Web API
│
Application
│
Infrastructure
│
Core
│
Microsoft SQL Server

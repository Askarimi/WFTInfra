# RPK.Infra

A clean architecture template for .NET applications.

## Project Structure

The solution is organized into four main layers:

### 1. Core Layer (RPK.Infra.Core)

- Contains business entities and interfaces
- Defines the core business logic
- No dependencies on other layers
- Folders:
  - Entities: Domain models
  - Interfaces: Repository and service contracts
  - ValueObjects: Domain value objects

### 2. Application Layer (RPK.Infra.Application)

- Implements use cases
- Orchestrates the flow of data
- Depends on Core layer
- Folders:
  - Commands: CQRS commands
  - Queries: CQRS queries
  - Interfaces: Application service contracts
  - DTOs: Data transfer objects

### 3. Infrastructure Layer (RPK.Infra.Infrastructure)

- Implements external services
- Handles data persistence
- Depends on Core and Application layers
- Folders:
  - Data: Database context and configurations
  - Repositories: Repository implementations
  - Services: External service implementations

### 4. Presentation Layer (RPK.Infra.WebApi)

- Handles HTTP requests
- Implements API controllers
- Depends on Application layer
- Folders:
  - Controllers: API endpoints
  - Middleware: Custom middleware
  - Extensions: Extension methods

## Getting Started

1. Clone the repository
2. Restore NuGet packages
3. Update the connection string in appsettings.json
4. Run the database migrations
5. Start the application

## Dependencies

- .NET 6.0
- Entity Framework Core
- MediatR
- AutoMapper
- FluentValidation

## Architecture Principles

- Dependency Inversion: High-level modules should not depend on low-level modules
- Single Responsibility: Each class should have only one reason to change
- Interface Segregation: Clients should not be forced to depend on interfaces they don't use
- Open/Closed: Software entities should be open for extension but closed for modification
- Liskov Substitution: Objects should be replaceable with instances of their subtypes

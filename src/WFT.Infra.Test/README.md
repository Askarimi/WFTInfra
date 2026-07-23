# RBAC + ABAC Access Control Tests

This document describes the comprehensive test suite for the combined Role-Based Access Control (RBAC) and Attribute-Based Access Control (ABAC) system implemented in the UserController.

## Test Structure

### 1. AuthorizationServiceTests.cs
Unit tests for the centralized `AuthorizationService` that combines RBAC and ABAC logic.

### 2. UsersControllerTests.cs
Integration tests for the `UsersController` to ensure authorization is properly enforced in all API endpoints.

### 3. ABACIntegrationTests.cs
Comprehensive integration tests that validate the actual ABAC policies and their enforcement.

## Test Scenarios

### RBAC (Role-Based Access Control) Tests

#### Permission-Based Access
- **CanCreateUser_WithoutPermission_ReturnsFalse**: Verifies that users without `CreateUser` permission cannot create users
- **CanViewUser_WithoutPermission_ReturnsFalse**: Verifies that users without `ViewUser` permission cannot view users
- **CanEditUser_WithoutPermission_ReturnsFalse**: Verifies that users without `EditUser` permission cannot edit users
- **CanDeleteUser_WithoutPermission_ReturnsFalse**: Verifies that users without `DeleteUser` permission cannot delete users

#### Permission-Based Access Success
- **CanCreateUser_WithValidPermission_ReturnsTrue**: Verifies that users with `CreateUser` permission can create users
- **CanViewUser_WithValidPermissionAndABAC_ReturnsTrue**: Verifies that users with `ViewUser` permission can view users (when ABAC also allows)
- **CanEditUser_WithValidPermissionAndABAC_ReturnsTrue**: Verifies that users with `EditUser` permission can edit users (when ABAC also allows)
- **CanDeleteUser_WithValidPermissionAndABAC_ReturnsTrue**: Verifies that users with `DeleteUser` permission can delete users (when ABAC also allows)

### ABAC (Attribute-Based Access Control) Tests

#### Department-Based Access Control
- **CanEditUser_SameDepartment_ReturnsTrue**: Verifies that users can edit other users in the same department
- **CanEditUser_DifferentDepartment_ReturnsFalse**: Verifies that users cannot edit users in different departments

#### Creator-Based Access Control
- **CanDeleteUser_UserCreatedByCurrentUser_ReturnsTrue**: Verifies that users can delete users they created
- **CanDeleteUser_UserNotCreatedByCurrentUser_ReturnsFalse**: Verifies that users cannot delete users they didn't create

### Combined RBAC + ABAC Tests

#### Successful Authorization
- **ValidateUserAccess_WithPermissionAndABAC_ReturnsAuthorized**: Tests the complete flow where both RBAC (permission) and ABAC (attributes) allow access

#### Failed Authorization
- **ValidateUserAccess_WithPermissionButABACDenies_ReturnsNotAuthorized**: Tests the scenario where RBAC allows but ABAC denies access
- **ValidateUserAccess_WithoutPermission_ReturnsNotAuthorized**: Tests the scenario where RBAC denies access (ABAC is not checked)

### Controller Integration Tests

#### Create User Endpoint
- **Create_WithValidAuthorization_ReturnsCreatedResponse**: Tests successful user creation with proper authorization
- **Create_WithoutAuthorization_ReturnsForbidden**: Tests user creation rejection due to lack of authorization

#### Get User Endpoint
- **GetById_WithValidAuthorization_ReturnsSuccessResponse**: Tests successful user retrieval with proper authorization
- **GetById_WithoutAuthorization_ReturnsForbidden**: Tests user retrieval rejection due to lack of authorization
- **GetById_UserNotFound_ReturnsNotFound**: Tests user retrieval when user doesn't exist

#### Get All Users Endpoint
- **GetAll_WithValidAuthorization_ReturnsSuccessResponse**: Tests successful user list retrieval with proper authorization and filtering
- **GetAll_WithoutAuthorization_ReturnsForbidden**: Tests user list retrieval rejection due to lack of authorization
- **GetAll_WithInvalidPageNumber_ReturnsBadRequest**: Tests validation of pagination parameters

#### Update User Endpoint
- **Update_WithValidAuthorization_ReturnsSuccessResponse**: Tests successful user update with proper authorization
- **Update_WithoutAuthorization_ReturnsForbidden**: Tests user update rejection due to lack of authorization

#### Delete User Endpoint
- **Delete_WithValidAuthorization_ReturnsNoContent**: Tests successful user deletion with proper authorization
- **Delete_WithoutAuthorization_ReturnsForbidden**: Tests user deletion rejection due to lack of authorization

### Accessible Users Filtering Tests

#### Department-Based Filtering
- **GetAccessibleUserIds_WithDepartmentRestriction_ReturnsFilteredUsers**: Tests that the system correctly filters users based on department access rules

### Edge Cases Tests

#### User Not Found Scenarios
- **ValidateUserAccess_TargetUserNotFound_ReturnsNotAuthorized**: Tests authorization when target user doesn't exist
- **ValidateUserAccess_CurrentUserNotFound_ReturnsNotAuthorized**: Tests authorization when current user doesn't exist

## Test Data Examples

### Example 1: Department-Based Access Control
```csharp
// User 1 (HR Department) tries to edit User 2 (HR Department) - ALLOWED
// User 1 (HR Department) tries to edit User 3 (IT Department) - DENIED
```

### Example 2: Creator-Based Access Control
```csharp
// User 1 tries to delete User 2 (created by User 1) - ALLOWED
// User 1 tries to delete User 3 (created by User 4) - DENIED
```

### Example 3: Combined RBAC + ABAC
```csharp
// User 1 has EditUser permission (RBAC) AND User 2 is in same department (ABAC) - ALLOWED
// User 1 has EditUser permission (RBAC) BUT User 2 is in different department (ABAC) - DENIED
// User 1 has NO EditUser permission (RBAC) - DENIED (ABAC not checked)
```

## Running the Tests

### Prerequisites
- .NET 9.0
- MSTest framework
- Moq library for mocking

### Command Line
```bash
cd src/WFT.Infra.Test
dotnet test
```

### Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer
3. Run all tests or specific test categories

## Test Categories

### Unit Tests
- **AuthorizationServiceTests**: Tests the authorization service logic in isolation
- **UsersControllerTests**: Tests controller actions with mocked dependencies

### Integration Tests
- **ABACIntegrationTests**: Tests the complete RBAC + ABAC integration

## Expected Test Results

### All Tests Should Pass
- ✅ RBAC permission checks work correctly
- ✅ ABAC attribute-based rules work correctly
- ✅ Combined RBAC + ABAC logic works correctly
- ✅ Controller endpoints enforce authorization properly
- ✅ Error handling works correctly for unauthorized access
- ✅ Edge cases are handled properly

### Test Coverage
- **AuthorizationService**: 100% method coverage
- **UsersController**: 100% action method coverage
- **ABAC Integration**: All major scenarios covered

## Security Validation

These tests ensure that:

1. **No unauthorized access**: Users without proper permissions cannot access protected resources
2. **Attribute-based restrictions**: Even users with permissions are restricted by attribute-based rules
3. **Proper error responses**: Unauthorized access returns appropriate HTTP status codes (403 Forbidden)
4. **Data filtering**: Users only see data they're authorized to access
5. **Audit trail**: All authorization decisions are properly logged and traceable

## Maintenance

### Adding New Tests
When adding new ABAC policies or RBAC permissions:

1. Add unit tests to `AuthorizationServiceTests.cs`
2. Add integration tests to `ABACIntegrationTests.cs`
3. Add controller tests to `UsersControllerTests.cs`
4. Update this documentation

### Test Data Management
- Use realistic test data that mirrors production scenarios
- Ensure test data covers edge cases and boundary conditions
- Keep test data consistent across related tests

## Performance Considerations

- Tests use mocking to avoid database dependencies
- Authorization checks are optimized to fail fast (RBAC first, then ABAC)
- Bulk operations (like `GetAccessibleUserIds`) are tested for performance impact

## Troubleshooting

### Common Issues
1. **Mock setup errors**: Ensure all dependencies are properly mocked
2. **Async/await issues**: All test methods should be async and properly await results
3. **Test isolation**: Each test should be independent and not rely on other tests

### Debugging
- Use `[TestMethod]` attributes for individual test execution
- Add logging to understand authorization decision flow
- Use breakpoints to step through authorization logic 
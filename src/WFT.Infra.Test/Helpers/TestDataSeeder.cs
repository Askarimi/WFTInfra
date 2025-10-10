using WFT.Infra.Core.Entities.UserManagment;
using WFT.Infra.Infrastructure.Data;

namespace WFT.Infra.Test.Helpers
{
    /// <summary>
    /// Helper class to seed test data for RBAC verification tests
    /// </summary>
    public static class TestDataSeeder
    {
        /// <summary>
        /// Seeds a complete RBAC structure with Users, Roles, Permissions, and their relationships
        /// </summary>
        public static void SeedRbacData(ApplicationDbContext context)
        {
            // Clear existing data
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Create Permissions
            var createUserPermission = new Permission
            {
                Id = 1,
                Name = "CreateUser",
                DisplayName = "Create User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var editUserPermission = new Permission
            {
                Id = 2,
                Name = "EditUser",
                DisplayName = "Edit User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var deleteUserPermission = new Permission
            {
                Id = 3,
                Name = "DeleteUser",
                DisplayName = "Delete User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var viewUserPermission = new Permission
            {
                Id = 4,
                Name = "ViewUser",
                DisplayName = "View User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var viewUserListPermission = new Permission
            {
                Id = 5,
                Name = "ViewUserList",
                DisplayName = "View User List",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var inactivePermission = new Permission
            {
                Id = 6,
                Name = "InactivePermission",
                DisplayName = "Inactive Permission",
                IsActive = false, // Inactive - should not be granted
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            context.Permissions.AddRange(
                createUserPermission,
                editUserPermission,
                deleteUserPermission,
                viewUserPermission,
                viewUserListPermission,
                inactivePermission
            );

            // Create Roles
            var adminRole = new Role
            {
                Id = 1,
                Name = "Administrator",
                Description = "Full system access",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var managerRole = new Role
            {
                Id = 2,
                Name = "Manager",
                Description = "Manager access",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var viewerRole = new Role
            {
                Id = 3,
                Name = "Viewer",
                Description = "Read-only access",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            context.Roles.AddRange(adminRole, managerRole, viewerRole);

            // Create Users
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var managerUser = new User
            {
                Id = 2,
                Username = "manager",
                Email = "manager@test.com",
                FirstName = "Manager",
                LastName = "User",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var viewerUser = new User
            {
                Id = 3,
                Username = "viewer",
                Email = "viewer@test.com",
                FirstName = "Viewer",
                LastName = "User",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            context.Users.AddRange(adminUser, managerUser, viewerUser);

            // Assign Roles to Users (UserRoles - Junction Table)
            var adminUserRole = new UserRole
            {
                Id = 1,
                UserId = 1,
                RoleId = 1, // Administrator
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var managerUserRole = new UserRole
            {
                Id = 2,
                UserId = 2,
                RoleId = 2, // Manager
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            var viewerUserRole = new UserRole
            {
                Id = 3,
                UserId = 3,
                RoleId = 3, // Viewer
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            };

            context.UserRoles.AddRange(adminUserRole, managerUserRole, viewerUserRole);

            // Assign Permissions to Roles (RolePermissions - Junction Table)
            
            // Administrator has all active permissions
            var adminRolePermissions = new List<RolePermission>
            {
                new RolePermission { Id = 1, RoleId = 1, PermissionId = 1, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // CreateUser
                new RolePermission { Id = 2, RoleId = 1, PermissionId = 2, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // EditUser
                new RolePermission { Id = 3, RoleId = 1, PermissionId = 3, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // DeleteUser
                new RolePermission { Id = 4, RoleId = 1, PermissionId = 4, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // ViewUser
                new RolePermission { Id = 5, RoleId = 1, PermissionId = 5, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // ViewUserList
            };

            // Manager has Create, Edit, View permissions (no Delete)
            var managerRolePermissions = new List<RolePermission>
            {
                new RolePermission { Id = 6, RoleId = 2, PermissionId = 1, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // CreateUser
                new RolePermission { Id = 7, RoleId = 2, PermissionId = 2, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // EditUser
                new RolePermission { Id = 8, RoleId = 2, PermissionId = 4, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // ViewUser
                new RolePermission { Id = 9, RoleId = 2, PermissionId = 5, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // ViewUserList
            };

            // Viewer has only View permissions
            var viewerRolePermissions = new List<RolePermission>
            {
                new RolePermission { Id = 10, RoleId = 3, PermissionId = 4, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // ViewUser
                new RolePermission { Id = 11, RoleId = 3, PermissionId = 5, CreatedAt = DateTime.UtcNow, RowVersion = new byte[8] }, // ViewUserList
            };

            context.RolePermissions.AddRange(adminRolePermissions);
            context.RolePermissions.AddRange(managerRolePermissions);
            context.RolePermissions.AddRange(viewerRolePermissions);

            context.SaveChanges();
        }

        /// <summary>
        /// Gets all active permission names for a specific user (for test verification)
        /// </summary>
        public static List<string> GetExpectedPermissionsForUser(long userId)
        {
            return userId switch
            {
                1 => new List<string> { "CreateUser", "EditUser", "DeleteUser", "ViewUser", "ViewUserList" }, // Admin
                2 => new List<string> { "CreateUser", "EditUser", "ViewUser", "ViewUserList" }, // Manager
                3 => new List<string> { "ViewUser", "ViewUserList" }, // Viewer
                _ => new List<string>()
            };
        }
    }
}


using Xunit;
using Microsoft.EntityFrameworkCore;
using WFT.Infra.Infrastructure.Data;
using WFT.Infra.Infrastructure.Repositories;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Test.Helpers;

namespace WFT.Infra.Test.RBAC
{
    /// <summary>
    /// Tests to verify RBAC (Role-Based Access Control) functionality
    /// Migrated from API diagnostic endpoint to proper unit tests
    /// </summary>
    public class RbacVerificationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;

        public RbacVerificationTests()
        {
            // Setup In-Memory Database for isolated testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test run
                .Options;

            _context = new ApplicationDbContext(options);
            
            // Seed test data
            TestDataSeeder.SeedRbacData(_context);

            // Initialize repository
            _userRepository = new UserRepository(_context);
        }

        /// <summary>
        /// Test: HasPermissionAsync should return TRUE for all active permissions assigned to the user
        /// </summary>
        [Fact]
        public async Task Should_Recognize_All_Active_Permissions()
        {
            // Arrange
            long adminUserId = 1; // Admin user from seeded data
            var expectedPermissions = TestDataSeeder.GetExpectedPermissionsForUser(adminUserId);

            // Act & Assert
            foreach (var permission in expectedPermissions)
            {
                var hasPermission = await _userRepository.HasPermissionAsync(adminUserId, permission);
                
                Assert.True(
                    hasPermission,
                    $"Expected user {adminUserId} to have permission '{permission}', but HasPermissionAsync returned false"
                );
            }
        }

        /// <summary>
        /// Test: HasPermissionAsync should return FALSE for permissions that do not exist or are not assigned
        /// </summary>
        [Fact]
        public async Task Should_Return_False_For_NonExistent_Permission()
        {
            // Arrange
            long adminUserId = 1;
            string nonExistentPermission = "NonExistentPermission_Test123";

            // Act
            var hasPermission = await _userRepository.HasPermissionAsync(adminUserId, nonExistentPermission);

            // Assert
            Assert.False(
                hasPermission,
                $"Expected HasPermissionAsync to return false for non-existent permission '{nonExistentPermission}', but it returned true"
            );
        }

        /// <summary>
        /// Test: HasPermissionAsync should be case-insensitive
        /// </summary>
        [Theory]
        [InlineData("EditUser")]
        [InlineData("edituser")]
        [InlineData("EDITUSER")]
        [InlineData("EdItUsEr")]
        public async Task Should_Be_Case_Insensitive(string permissionVariation)
        {
            // Arrange
            long adminUserId = 1;

            // Act
            var hasPermission = await _userRepository.HasPermissionAsync(adminUserId, permissionVariation);

            // Assert
            Assert.True(
                hasPermission,
                $"Expected case-insensitive match for permission '{permissionVariation}', but HasPermissionAsync returned false"
            );
        }

        /// <summary>
        /// Test: HasPermissionAsync should trim whitespace from permission names
        /// </summary>
        [Theory]
        [InlineData(" EditUser")]
        [InlineData("EditUser ")]
        [InlineData(" EditUser ")]
        [InlineData("  EditUser  ")]
        public async Task Should_Trim_Whitespace_From_Permission_Name(string permissionWithWhitespace)
        {
            // Arrange
            long adminUserId = 1;

            // Act
            var hasPermission = await _userRepository.HasPermissionAsync(adminUserId, permissionWithWhitespace);

            // Assert
            Assert.True(
                hasPermission,
                $"Expected whitespace to be trimmed for permission '{permissionWithWhitespace}', but HasPermissionAsync returned false"
            );
        }

        /// <summary>
        /// Test: HasPermissionAsync should return FALSE for inactive permissions
        /// </summary>
        [Fact]
        public async Task Should_Not_Grant_Inactive_Permissions()
        {
            // Arrange
            long adminUserId = 1;
            string inactivePermission = "InactivePermission"; // This is marked as IsActive = false in seed data

            // Act
            var hasPermission = await _userRepository.HasPermissionAsync(adminUserId, inactivePermission);

            // Assert
            Assert.False(
                hasPermission,
                "Expected HasPermissionAsync to return false for inactive permission, but it returned true"
            );
        }

        /// <summary>
        /// Test: Manager user should NOT have DeleteUser permission
        /// </summary>
        [Fact]
        public async Task Manager_Should_Not_Have_Delete_Permission()
        {
            // Arrange
            long managerUserId = 2;
            string deletePermission = "DeleteUser";

            // Act
            var hasPermission = await _userRepository.HasPermissionAsync(managerUserId, deletePermission);

            // Assert
            Assert.False(
                hasPermission,
                "Manager should not have DeleteUser permission, but HasPermissionAsync returned true"
            );
        }

        /// <summary>
        /// Test: Manager user SHOULD have EditUser permission
        /// </summary>
        [Fact]
        public async Task Manager_Should_Have_Edit_Permission()
        {
            // Arrange
            long managerUserId = 2;
            string editPermission = "EditUser";

            // Act
            var hasPermission = await _userRepository.HasPermissionAsync(managerUserId, editPermission);

            // Assert
            Assert.True(
                hasPermission,
                "Manager should have EditUser permission, but HasPermissionAsync returned false"
            );
        }

        /// <summary>
        /// Test: Viewer user should only have view permissions
        /// </summary>
        [Fact]
        public async Task Viewer_Should_Only_Have_View_Permissions()
        {
            // Arrange
            long viewerUserId = 3;
            var expectedPermissions = new[] { "ViewUser", "ViewUserList" };
            var deniedPermissions = new[] { "CreateUser", "EditUser", "DeleteUser" };

            // Act & Assert - Expected permissions
            foreach (var permission in expectedPermissions)
            {
                var hasPermission = await _userRepository.HasPermissionAsync(viewerUserId, permission);
                Assert.True(hasPermission, $"Viewer should have '{permission}' permission");
            }

            // Act & Assert - Denied permissions
            foreach (var permission in deniedPermissions)
            {
                var hasPermission = await _userRepository.HasPermissionAsync(viewerUserId, permission);
                Assert.False(hasPermission, $"Viewer should NOT have '{permission}' permission");
            }
        }

        /// <summary>
        /// Test: Comprehensive verification - all users with all their expected permissions
        /// </summary>
        [Fact]
        public async Task Comprehensive_RBAC_Verification_All_Users()
        {
            // Arrange
            var userTests = new[]
            {
                new { UserId = 1L, Username = "Admin", Permissions = TestDataSeeder.GetExpectedPermissionsForUser(1) },
                new { UserId = 2L, Username = "Manager", Permissions = TestDataSeeder.GetExpectedPermissionsForUser(2) },
                new { UserId = 3L, Username = "Viewer", Permissions = TestDataSeeder.GetExpectedPermissionsForUser(3) }
            };

            int totalTests = 0;
            int passedTests = 0;

            // Act & Assert
            foreach (var userTest in userTests)
            {
                foreach (var expectedPermission in userTest.Permissions)
                {
                    totalTests++;
                    var hasPermission = await _userRepository.HasPermissionAsync(userTest.UserId, expectedPermission);
                    
                    if (hasPermission)
                    {
                        passedTests++;
                    }
                    else
                    {
                        Assert.Fail($"FAIL: {userTest.Username} (ID:{userTest.UserId}) should have '{expectedPermission}' permission");
                    }
                }

                // Also test that they DON'T have a non-existent permission
                totalTests++;
                var shouldBeFalse = await _userRepository.HasPermissionAsync(userTest.UserId, "NonExistentPermission");
                if (!shouldBeFalse)
                {
                    passedTests++;
                }
                else
                {
                    Assert.Fail($"FAIL: {userTest.Username} should NOT have 'NonExistentPermission'");
                }
            }

            // Final assertion - all tests should pass
            Assert.Equal(totalTests, passedTests);
        }

        /// <summary>
        /// Test: Verify that HasPermissionAsync doesn't require .Include() calls
        /// This is verified by the fact that we're not using .Include() anywhere and tests pass
        /// </summary>
        [Fact]
        public async Task Should_Work_Without_Include_Calls()
        {
            // Arrange
            long adminUserId = 1;
            
            // Act - We're NOT using .Include() anywhere in the repository
            var hasCreatePermission = await _userRepository.HasPermissionAsync(adminUserId, "CreateUser");
            var hasEditPermission = await _userRepository.HasPermissionAsync(adminUserId, "EditUser");
            
            // Assert - Both should work because SelectMany() generates proper JOINs
            Assert.True(hasCreatePermission, "Should recognize CreateUser permission without .Include()");
            Assert.True(hasEditPermission, "Should recognize EditUser permission without .Include()");
        }

        [Fact]
        public async Task SuperAdmin_Should_Bypass_Rbac_And_Abac_Without_Assigned_Permissions()
        {
            var userService = new Moq.Mock<WFT.Infra.Application.Contracts.Interfaces.UserManagment.IUserService>(Moq.MockBehavior.Strict);
            userService
                .Setup(service => service.GetRolesForUserAsync(99))
                .Returns(Task.FromResult<IEnumerable<WFT.Infra.Application.Contracts.DTOs.UserManagment.RoleDto>>(
                [
                    new WFT.Infra.Application.Contracts.DTOs.UserManagment.RoleDto
                    {
                        Name = "SuperAdmin",
                        IsActive = true
                    }
                ]));

            var abacService = new Moq.Mock<WFT.Infra.Application.Contracts.Interfaces.UserManagment.IABACService>(Moq.MockBehavior.Strict);
            var repository = Moq.Mock.Of<WFT.Infra.Application.Contracts.Repositories.IRepository<WFT.Infra.Core.Entities.UserManagment.User>>();
            var authorizationService = new WFT.Infra.Application.Services.AuthorizationService(
                userService.Object,
                abacService.Object,
                repository);

            var hasAccess = await authorizationService.HasPermissionAsync(
                99,
                "PermissionWithoutAssignment",
                new { Id = 10 });
            var detailedResult = await authorizationService.EvaluateAccessDetailedAsync(
                99,
                "PermissionWithoutAssignment",
                new { Id = 10 });

            Assert.True(hasAccess);
            Assert.True(detailedResult.HasAccess);
            Assert.Contains("SuperAdmin", detailedResult.EvaluationReason);
            userService.Verify(
                service => service.HasPermissionAsync(Moq.It.IsAny<long>(), Moq.It.IsAny<string>()),
                Moq.Times.Never);
            abacService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Normal_Role_Should_Continue_Using_Explicit_Permission_Checks()
        {
            var userService = new Moq.Mock<WFT.Infra.Application.Contracts.Interfaces.UserManagment.IUserService>(Moq.MockBehavior.Strict);
            userService
                .Setup(service => service.GetRolesForUserAsync(100))
                .Returns(Task.FromResult<IEnumerable<WFT.Infra.Application.Contracts.DTOs.UserManagment.RoleDto>>(
                [
                    new WFT.Infra.Application.Contracts.DTOs.UserManagment.RoleDto
                    {
                        Name = "Manager",
                        IsActive = true
                    }
                ]));
            userService
                .Setup(service => service.HasPermissionAsync(100, "DeleteUser"))
                .Returns(Task.FromResult(false));

            var abacService = new Moq.Mock<WFT.Infra.Application.Contracts.Interfaces.UserManagment.IABACService>(Moq.MockBehavior.Strict);
            var repository = Moq.Mock.Of<WFT.Infra.Application.Contracts.Repositories.IRepository<WFT.Infra.Core.Entities.UserManagment.User>>();
            var authorizationService = new WFT.Infra.Application.Services.AuthorizationService(
                userService.Object,
                abacService.Object,
                repository);

            var hasAccess = await authorizationService.HasPermissionAsync(100, "DeleteUser");

            Assert.False(hasAccess);
            userService.Verify(
                service => service.HasPermissionAsync(100, "DeleteUser"),
                Moq.Times.Once);
            abacService.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Cleanup: Dispose of the in-memory database context
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}




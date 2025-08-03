using Moq;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Application.Services;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Test
{
    [TestClass]
    public class ABACIntegrationTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<IABACService> _mockABACService;
        private Mock<IRepository<User>> _mockUserRepository;
        private AuthorizationService _authorizationService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockABACService = new Mock<IABACService>();
            _mockUserRepository = new Mock<IRepository<User>>();

            _authorizationService = new AuthorizationService(
                _mockUserService.Object,
                _mockABACService.Object,
                _mockUserRepository.Object);
        }

        #region Department-Based Access Control Tests

        [TestMethod]
        public async Task CanEditUser_SameDepartment_ReturnsTrue()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;

            // User has EditUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "EditUser"))
                .ReturnsAsync(true);

            // ABAC allows editing users in same department
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "EditUser", targetUserId, "User"))
                .ReturnsAsync(true);

            // Act
            var result = await _authorizationService.CanEditUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanEditUser_DifferentDepartment_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;

            // User has EditUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "EditUser"))
                .ReturnsAsync(true);

            // ABAC denies editing users in different department
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "EditUser", targetUserId, "User"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanEditUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Creator-Based Access Control Tests

        [TestMethod]
        public async Task CanDeleteUser_UserCreatedByCurrentUser_ReturnsTrue()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;

            // User has DeleteUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "DeleteUser"))
                .ReturnsAsync(true);

            // ABAC allows deleting users created by current user
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "DeleteUser", targetUserId, "User"))
                .ReturnsAsync(true);

            // Act
            var result = await _authorizationService.CanDeleteUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanDeleteUser_UserNotCreatedByCurrentUser_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;

            // User has DeleteUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "DeleteUser"))
                .ReturnsAsync(true);

            // ABAC denies deleting users not created by current user
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "DeleteUser", targetUserId, "User"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanDeleteUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Role-Based Access Control Tests

        [TestMethod]
        public async Task CanCreateUser_WithoutPermission_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;

            // User does not have CreateUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "CreateUser"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanCreateUserAsync(currentUserId);

            // Assert
            Assert.IsFalse(result);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public async Task CanViewUser_WithoutPermission_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;

            // User does not have ViewUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "ViewUser"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanViewUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsFalse(result);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        #endregion

        #region Combined RBAC + ABAC Tests

        [TestMethod]
        public async Task ValidateUserAccess_WithPermissionAndABAC_ReturnsAuthorized()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            var targetUser = new User { Id = targetUserId, CreatedUserId = currentUserId };
            var currentUser = new User { Id = currentUserId };

            _mockUserRepository.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync(targetUser);
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId))
                .ReturnsAsync(currentUser);

            // RBAC: User has EditUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "EditUser"))
                .ReturnsAsync(true);

            // ABAC: User can edit users in same department
            var abacResult = new ABACEvaluationResponseDto
            {
                HasAccess = true,
                PolicyResults = new List<PolicyEvaluationResultDto>
                {
                    new PolicyEvaluationResultDto
                    {
                        PolicyRuleId = 1,
                        PolicyName = "SameDepartmentEdit",
                        Effect = "Allow",
                        IsApplicable = true,
                        ConditionsMet = true,
                        Reason = "User is in the same department"
                    }
                }
            };

            _mockABACService.Setup(x => x.EvaluateAccessDetailedAsync(currentUserId, "EditUser", targetUserId, "User"))
                .ReturnsAsync(abacResult);

            // Act
            var result = await _authorizationService.ValidateUserAccessAsync(currentUserId, targetUserId, "EditUser");

            // Assert
            Assert.IsTrue(result.IsAuthorized);
        }

        [TestMethod]
        public async Task ValidateUserAccess_WithPermissionButABACDenies_ReturnsNotAuthorized()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            var targetUser = new User { Id = targetUserId, CreatedUserId = 999L }; // Created by different user
            var currentUser = new User { Id = currentUserId };

            _mockUserRepository.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync(targetUser);
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId))
                .ReturnsAsync(currentUser);

            // RBAC: User has DeleteUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "DeleteUser"))
                .ReturnsAsync(true);

            // ABAC: User cannot delete users not created by them
            var abacResult = new ABACEvaluationResponseDto
            {
                HasAccess = false,
                PolicyResults = new List<PolicyEvaluationResultDto>
                {
                    new PolicyEvaluationResultDto
                    {
                        PolicyRuleId = 1,
                        PolicyName = "CreatorOnlyDelete",
                        Effect = "Allow",
                        IsApplicable = true,
                        ConditionsMet = false,
                        Reason = "User can only delete users they created"
                    }
                }
            };

            _mockABACService.Setup(x => x.EvaluateAccessDetailedAsync(currentUserId, "DeleteUser", targetUserId, "User"))
                .ReturnsAsync(abacResult);

            // Act
            var result = await _authorizationService.ValidateUserAccessAsync(currentUserId, targetUserId, "DeleteUser");

            // Assert
            Assert.IsFalse(result.IsAuthorized);
            Assert.AreEqual("دسترسی بر اساس ویژگی‌ها محدود شده است.", result.Reason);
        }

        #endregion

        #region Accessible Users Filtering Tests

        [TestMethod]
        public async Task GetAccessibleUserIds_WithDepartmentRestriction_ReturnsFilteredUsers()
        {
            // Arrange
            var currentUserId = 1L;
            var allUsers = new List<User>
            {
                new User { Id = 1, CreatedUserId = 999L }, // Same department
                new User { Id = 2, CreatedUserId = 999L }, // Same department
                new User { Id = 3, CreatedUserId = 999L }, // Different department
                new User { Id = 4, CreatedUserId = 999L }  // Same department
            };

            _mockUserRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(allUsers);

            // RBAC: User has ViewUser permission
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "ViewUser"))
                .ReturnsAsync(true);

            // ABAC: Can view users 1, 2, 4 (same department) but not 3 (different department)
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "ViewUser", 1, "User"))
                .ReturnsAsync(true);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "ViewUser", 2, "User"))
                .ReturnsAsync(true);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "ViewUser", 3, "User"))
                .ReturnsAsync(false);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "ViewUser", 4, "User"))
                .ReturnsAsync(true);

            // Act
            var result = await _authorizationService.GetAccessibleUserIdsAsync(currentUserId);

            // Assert
            var accessibleIds = result.ToList();
            Assert.AreEqual(3, accessibleIds.Count);
            CollectionAssert.Contains(accessibleIds, 1L);
            CollectionAssert.Contains(accessibleIds, 2L);
            CollectionAssert.Contains(accessibleIds, 4L);
            CollectionAssert.DoesNotContain(accessibleIds, 3L);
        }

        #endregion

        #region Edge Cases Tests

        [TestMethod]
        public async Task ValidateUserAccess_TargetUserNotFound_ReturnsNotAuthorized()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 999L;

            _mockUserRepository.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authorizationService.ValidateUserAccessAsync(currentUserId, targetUserId, "EditUser");

            // Assert
            Assert.IsFalse(result.IsAuthorized);
            Assert.AreEqual("کاربر مورد نظر یافت نشد.", result.Reason);
        }

        [TestMethod]
        public async Task ValidateUserAccess_CurrentUserNotFound_ReturnsNotAuthorized()
        {
            // Arrange
            var currentUserId = 999L;
            var targetUserId = 2L;
            var targetUser = new User { Id = targetUserId };

            _mockUserRepository.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync(targetUser);
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authorizationService.ValidateUserAccessAsync(currentUserId, targetUserId, "EditUser");

            // Assert
            Assert.IsFalse(result.IsAuthorized);
            Assert.AreEqual("کاربر جاری یافت نشد.", result.Reason);
        }

        #endregion
    }
}
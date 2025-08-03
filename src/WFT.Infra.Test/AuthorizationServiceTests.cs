using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Application.Services;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Test
{
    [TestClass]
    public class AuthorizationServiceTests
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

        #region CanCreateUserAsync Tests

        [TestMethod]
        public async Task CanCreateUserAsync_WithValidPermission_ReturnsTrue()
        {
            // Arrange
            var currentUserId = 1L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "CreateUser"))
                .ReturnsAsync(true);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "CreateUser", null, null))
                .ReturnsAsync(true);

            // Act
            var result = await _authorizationService.CanCreateUserAsync(currentUserId);

            // Assert
            Assert.IsTrue(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "CreateUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(currentUserId, "CreateUser", null, null), Times.Once);
        }

        [TestMethod]
        public async Task CanCreateUserAsync_WithoutPermission_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "CreateUser"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanCreateUserAsync(currentUserId);

            // Assert
            Assert.IsFalse(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "CreateUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public async Task CanCreateUserAsync_WithPermissionButABACDenies_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "CreateUser"))
                .ReturnsAsync(true);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "CreateUser", null, null))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanCreateUserAsync(currentUserId);

            // Assert
            Assert.IsFalse(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "CreateUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(currentUserId, "CreateUser", null, null), Times.Once);
        }

        #endregion

        #region CanViewUserAsync Tests

        [TestMethod]
        public async Task CanViewUserAsync_WithValidPermissionAndABAC_ReturnsTrue()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "ViewUser"))
                .ReturnsAsync(true);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "ViewUser", targetUserId, "User"))
                .ReturnsAsync(true);

            // Act
            var result = await _authorizationService.CanViewUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsTrue(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "ViewUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(currentUserId, "ViewUser", targetUserId, "User"), Times.Once);
        }

        [TestMethod]
        public async Task CanViewUserAsync_WithoutPermission_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "ViewUser"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanViewUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsFalse(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "ViewUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        #endregion

        #region CanEditUserAsync Tests

        [TestMethod]
        public async Task CanEditUserAsync_WithValidPermissionAndABAC_ReturnsTrue()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "EditUser"))
                .ReturnsAsync(true);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "EditUser", targetUserId, "User"))
                .ReturnsAsync(true);

            // Act
            var result = await _authorizationService.CanEditUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsTrue(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "EditUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(currentUserId, "EditUser", targetUserId, "User"), Times.Once);
        }

        [TestMethod]
        public async Task CanEditUserAsync_WithoutPermission_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "EditUser"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanEditUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsFalse(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "EditUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        #endregion

        #region CanDeleteUserAsync Tests

        [TestMethod]
        public async Task CanDeleteUserAsync_WithValidPermissionAndABAC_ReturnsTrue()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "DeleteUser"))
                .ReturnsAsync(true);
            _mockABACService.Setup(x => x.EvaluateAccessAsync(currentUserId, "DeleteUser", targetUserId, "User"))
                .ReturnsAsync(true);

            // Act
            var result = await _authorizationService.CanDeleteUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsTrue(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "DeleteUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(currentUserId, "DeleteUser", targetUserId, "User"), Times.Once);
        }

        [TestMethod]
        public async Task CanDeleteUserAsync_WithoutPermission_ReturnsFalse()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "DeleteUser"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.CanDeleteUserAsync(currentUserId, targetUserId);

            // Assert
            Assert.IsFalse(result);
            _mockUserService.Verify(x => x.HasPermissionAsync(currentUserId, "DeleteUser"), Times.Once);
            _mockABACService.Verify(x => x.EvaluateAccessAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        }

        #endregion

        #region ValidateUserAccessAsync Tests

        [TestMethod]
        public async Task ValidateUserAccessAsync_TargetUserNotFound_ReturnsNotAuthorized()
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
        public async Task ValidateUserAccessAsync_CurrentUserNotFound_ReturnsNotAuthorized()
        {
            // Arrange
            var currentUserId = 999L;
            var targetUserId = 2L;
            var targetUser = new User { Id = targetUserId };
            var currentUser = (User)null;

            _mockUserRepository.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync(targetUser);
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId))
                .ReturnsAsync(currentUser);

            // Act
            var result = await _authorizationService.ValidateUserAccessAsync(currentUserId, targetUserId, "EditUser");

            // Assert
            Assert.IsFalse(result.IsAuthorized);
            Assert.AreEqual("کاربر جاری یافت نشد.", result.Reason);
        }

        [TestMethod]
        public async Task ValidateUserAccessAsync_WithoutPermission_ReturnsNotAuthorized()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            var targetUser = new User { Id = targetUserId };
            var currentUser = new User { Id = currentUserId };

            _mockUserRepository.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync(targetUser);
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId))
                .ReturnsAsync(currentUser);
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "EditUser"))
                .ReturnsAsync(false);

            // Act
            var result = await _authorizationService.ValidateUserAccessAsync(currentUserId, targetUserId, "EditUser");

            // Assert
            Assert.IsFalse(result.IsAuthorized);
            Assert.AreEqual("شما مجوز انجام این عملیات را ندارید.", result.Reason);
            Assert.AreEqual("EditUser", result.RequiredPermission);
        }

        [TestMethod]
        public async Task ValidateUserAccessAsync_WithPermissionAndABAC_ReturnsAuthorized()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;
            var targetUser = new User { Id = targetUserId };
            var currentUser = new User { Id = currentUserId };

            _mockUserRepository.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync(targetUser);
            _mockUserRepository.Setup(x => x.GetByIdAsync(currentUserId))
                .ReturnsAsync(currentUser);
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "EditUser"))
                .ReturnsAsync(true);

            var abacResult = new WFT.Infra.Application.Contracts.DTOs.UserManagment.ABACEvaluationResponseDto
            {
                HasAccess = true,
                PolicyResults = new List<WFT.Infra.Application.Contracts.DTOs.UserManagment.PolicyEvaluationResultDto>()
            };

            _mockABACService.Setup(x => x.EvaluateAccessDetailedAsync(currentUserId, "EditUser", targetUserId, "User"))
                .ReturnsAsync(abacResult);

            // Act
            var result = await _authorizationService.ValidateUserAccessAsync(currentUserId, targetUserId, "EditUser");

            // Assert
            Assert.IsTrue(result.IsAuthorized);
        }

        #endregion

        #region GetAccessibleUserIdsAsync Tests

        [TestMethod]
        public async Task GetAccessibleUserIdsAsync_ReturnsOnlyAccessibleUsers()
        {
            // Arrange
            var currentUserId = 1L;
            var allUsers = new List<User>
            {
                new User { Id = 1 },
                new User { Id = 2 },
                new User { Id = 3 },
                new User { Id = 4 }
            };

            _mockUserRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(allUsers);

            // Mock CanViewUserAsync to return true for users 1, 2, 4 and false for user 3
            _mockUserService.Setup(x => x.HasPermissionAsync(currentUserId, "ViewUser"))
                .ReturnsAsync(true);

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
    }
} 
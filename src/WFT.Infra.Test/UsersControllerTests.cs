using Microsoft.AspNetCore.Mvc;
using Moq;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;
using WFT.Infra.WebApi.Controllers;

namespace WFT.Infra.Test
{
    [TestClass]
    public class UsersControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<IWorkContext> _mockWorkContext;
        private Mock<IAuthorizationService> _mockAuthorizationService;
        private UsersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockWorkContext = new Mock<IWorkContext>();
            _mockAuthorizationService = new Mock<IAuthorizationService>();

            _controller = new UsersController(
                _mockUserService.Object,
                _mockWorkContext.Object,
                _mockAuthorizationService.Object);
        }

        #region Create Tests

        [TestMethod]
        public async Task Create_WithValidAuthorization_ReturnsCreatedResponse()
        {
            // Arrange
            var currentUserId = 1L;
            var request = new UserDto
            {
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var createdUser = new UserDto
            {
                Id = 2,
                Username = "testuser",
                Email = "test@example.com"
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanCreateUserAsync(currentUserId))
                .ReturnsAsync(true);
            _mockUserService.Setup(x => x.AddAsync(It.IsAny<UserDto>()))
                .ReturnsAsync(createdUser);
            _mockUserService.Setup(x => x.GetByIdAsync(createdUser.Id))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
            var createdAtResult = result as CreatedAtActionResult;
            Assert.AreEqual(201, createdAtResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanCreateUserAsync(currentUserId), Times.Once);
            _mockUserService.Verify(x => x.AddAsync(It.IsAny<UserDto>()), Times.Once);
        }

        [TestMethod]
        public async Task Create_WithoutAuthorization_ReturnsForbidden()
        {
            // Arrange
            var currentUserId = 1L;
            var request = new UserDto
            {
                Username = "testuser",
                Email = "test@example.com"
            };

            var authResult = new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = "شما مجوز ایجاد کاربر را ندارید."
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanCreateUserAsync(currentUserId))
                .ReturnsAsync(false);
            _mockAuthorizationService.Setup(x => x.ValidateUserAccessAsync(currentUserId, 0, "CreateUser"))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.AreEqual(403, objectResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanCreateUserAsync(currentUserId), Times.Once);
            _mockUserService.Verify(x => x.AddAsync(It.IsAny<UserDto>()), Times.Never);
        }

        #endregion

        #region GetById Tests

        [TestMethod]
        public async Task GetById_WithValidAuthorization_ReturnsSuccessResponse()
        {
            // Arrange
            var currentUserId = 1L;
            int targetUserId = 2;
            var user = new UserDto
            {
                Id = targetUserId,
                Username = "testuser",
                Email = "test@example.com"
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanViewUserAsync(currentUserId, targetUserId))
                .ReturnsAsync(true);
            _mockUserService.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetById(targetUserId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(200, okResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanViewUserAsync(currentUserId, targetUserId), Times.Once);
            _mockUserService.Verify(x => x.GetByIdAsync(targetUserId), Times.Once);
        }

        [TestMethod]
        public async Task GetById_WithoutAuthorization_ReturnsForbidden()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2;

            var authResult = new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = "شما مجوز مشاهده این کاربر را ندارید."
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanViewUserAsync(currentUserId, targetUserId))
                .ReturnsAsync(false);
            _mockAuthorizationService.Setup(x => x.ValidateUserAccessAsync(currentUserId, targetUserId, "ViewUser"))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.GetById(targetUserId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.AreEqual(403, objectResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanViewUserAsync(currentUserId, targetUserId), Times.Once);
            _mockUserService.Verify(x => x.GetByIdAsync(It.IsAny<long>()), Times.Never);
        }

        [TestMethod]
        public async Task GetById_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 999;

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanViewUserAsync(currentUserId, targetUserId))
                .ReturnsAsync(true);
            _mockUserService.Setup(x => x.GetByIdAsync(targetUserId))
                .ReturnsAsync((UserDto)null);

            // Act
            var result = await _controller.GetById(targetUserId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.AreEqual(404, objectResult?.StatusCode);
        }

        #endregion

        #region GetAll Tests

        [TestMethod]
        public async Task GetAll_WithValidAuthorization_ReturnsSuccessResponse()
        {
            // Arrange
            var currentUserId = 1L;
            var request = new PagedQueryRequest
            {
                PageNumber = 1,
                PageSize = 10
            };

            var accessibleUserIds = new List<long> { 1, 2, 3 };
            var pagedResult = new PagedList<UserDto>(
                new List<UserDto> { new UserDto { Id = 1 }, new UserDto { Id = 2 } },
                2, 1, 10);

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanViewUserListAsync(currentUserId))
                .ReturnsAsync(true);
            _mockAuthorizationService.Setup(x => x.GetAccessibleUserIdsAsync(currentUserId))
                .ReturnsAsync(accessibleUserIds);
            _mockUserService.Setup(x => x.GetPagedListAsync(It.IsAny<PagedQueryRequest>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAll(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(200, okResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanViewUserListAsync(currentUserId), Times.Once);
            _mockAuthorizationService.Verify(x => x.GetAccessibleUserIdsAsync(currentUserId), Times.Once);
            _mockUserService.Verify(x => x.GetPagedListAsync(It.IsAny<PagedQueryRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAll_WithoutAuthorization_ReturnsForbidden()
        {
            // Arrange
            var currentUserId = 1L;
            var request = new PagedQueryRequest
            {
                PageNumber = 1,
                PageSize = 10
            };

            var authResult = new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = "شما مجوز مشاهده لیست کاربران را ندارید."
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanViewUserListAsync(currentUserId))
                .ReturnsAsync(false);
            _mockAuthorizationService.Setup(x => x.ValidateUserAccessAsync(currentUserId, 0, "ViewUserList"))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.GetAll(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.AreEqual(403, objectResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanViewUserListAsync(currentUserId), Times.Once);
            _mockUserService.Verify(x => x.GetPagedListAsync(It.IsAny<PagedQueryRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task GetAll_WithInvalidPageNumber_ReturnsBadRequest()
        {
            // Arrange
            var request = new PagedQueryRequest
            {
                PageNumber = 0,
                PageSize = 10
            };

            // Act
            var result = await _controller.GetAll(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual(400, badRequestResult?.StatusCode);
        }

        #endregion

        #region Update Tests

        [TestMethod]
        public async Task Update_WithValidAuthorization_ReturnsSuccessResponse()
        {
            // Arrange
            var currentUserId = 1L;
            var request = new UserDto
            {
                Id = 2,
                Username = "updateduser",
                Email = "updated@example.com"
            };

            var updatedUser = new UserDto
            {
                Id = 2,
                Username = "updateduser",
                Email = "updated@example.com"
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanEditUserAsync(currentUserId, request.Id))
                .ReturnsAsync(true);
            _mockUserService.Setup(x => x.UpdateAsync(request))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.Update(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(200, okResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanEditUserAsync(currentUserId, request.Id), Times.Once);
            _mockUserService.Verify(x => x.UpdateAsync(request), Times.Once);
        }

        [TestMethod]
        public async Task Update_WithoutAuthorization_ReturnsForbidden()
        {
            // Arrange
            var currentUserId = 1L;
            var request = new UserDto
            {
                Id = 2,
                Username = "updateduser",
                Email = "updated@example.com"
            };

            var authResult = new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = "شما مجوز ویرایش این کاربر را ندارید."
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanEditUserAsync(currentUserId, request.Id))
                .ReturnsAsync(false);
            _mockAuthorizationService.Setup(x => x.ValidateUserAccessAsync(currentUserId, request.Id, "EditUser"))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Update(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.AreEqual(403, objectResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanEditUserAsync(currentUserId, request.Id), Times.Once);
            _mockUserService.Verify(x => x.UpdateAsync(It.IsAny<UserDto>()), Times.Never);
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public async Task Delete_WithValidAuthorization_ReturnsNoContent()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanDeleteUserAsync(currentUserId, targetUserId))
                .ReturnsAsync(true);
            _mockUserService.Setup(x => x.DeleteAsync(targetUserId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(targetUserId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            var noContentResult = result as NoContentResult;
            Assert.AreEqual(204, noContentResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanDeleteUserAsync(currentUserId, targetUserId), Times.Once);
            _mockUserService.Verify(x => x.DeleteAsync(targetUserId), Times.Once);
        }

        [TestMethod]
        public async Task Delete_WithoutAuthorization_ReturnsForbidden()
        {
            // Arrange
            var currentUserId = 1L;
            var targetUserId = 2L;

            var authResult = new AuthorizationResult
            {
                IsAuthorized = false,
                Reason = "شما مجوز حذف این کاربر را ندارید."
            };

            _mockWorkContext.Setup(x => x.UserId).Returns(currentUserId);
            _mockAuthorizationService.Setup(x => x.CanDeleteUserAsync(currentUserId, targetUserId))
                .ReturnsAsync(false);
            _mockAuthorizationService.Setup(x => x.ValidateUserAccessAsync(currentUserId, targetUserId, "DeleteUser"))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Delete(targetUserId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.AreEqual(403, objectResult?.StatusCode);

            _mockAuthorizationService.Verify(x => x.CanDeleteUserAsync(currentUserId, targetUserId), Times.Once);
            _mockUserService.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
        }

        #endregion
    }
}
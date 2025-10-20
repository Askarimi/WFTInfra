using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;
using WFT.Infra.WebApi.CustomConfig;

namespace WFT.Infra.WebApi.Controllers
{

    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public UsersController(
            IUserService userService,
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _userService = userService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            // Authorization Check: Can current user create users?
            var currentUserId = _workContext.UserId.Value;


            // 1️⃣ بررسی دسترسی RBAC
            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreateUser");
            if (!hasPermission)
            {
                // اگر نیاز به جزئیات داشتیم، می‌توانیم از EvaluateAccessDetailedAsync استفاده کنیم
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreateUser");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد کاربر را ندارید.");
            }


            request.CreatedUserId = currentUserId;

            var user = await _userService.AddAsync(request);
            var result = await _userService.GetByIdAsync(user.Id);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
        }

        // READ BY ID
        [HttpGet()]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewUser");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewUser", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده این کاربر را ندارید.");
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound("کاربر یافت نشد.");

            return Ok(user);
        }

        // READ ALL
        [HttpGet("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewUserList");
            if (!authResult.HasAccess)
                return WFTJsonResult.Fail("Access denied", 403);

            if (request.PageNumber <= 0)
                return WFTJsonResult.Fail("PageNumber must be greater than 0", 400);

            if (request.PageSize <= 0)
                return WFTJsonResult.Fail("PageSize must be greater than 0", 400);

            var result = await _userService.GetPagedListAsync(request);



            return WFTJsonResult.Ok(result.Items, new
            {
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                totalPages = result.TotalPages,
                hasNextPage = result.HasNextPage,
                hasPreviousPage = result.HasPreviousPage
            });
        }


        // UPDATE
        [HttpPut()]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditUser", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این کاربر را ندارید.");

            var result = await _userService.UpdateAsync(request);

            return Ok(result);
        }

        // DELETE
        [HttpDelete()]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeleteUser");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeleteUser", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این کاربر را ندارید.");
            }

            await _userService.DeleteAsync(id);

            return NoContent();
        }

        /* 
         * DIAGNOSTIC ENDPOINTS REMOVED - Moved to Unit Tests
         * 
         * The following diagnostic endpoints have been moved to proper unit tests:
         * - test-rbac/{userId} → RbacVerificationTests.cs
         * - verify-permission/{userId}/{permissionName} → RbacVerificationTests.cs
         * - verify-rbac-fix/{userId} → RbacVerificationTests.cs
         * 
         * These endpoints were used for manual testing during development.
         * All RBAC verification logic now exists as automated tests in:
         * src/WFT.Infra.Test/Authorization/RbacVerificationTests.cs
         * 
         * To run tests: dotnet test src/WFT.Infra.Test
         */
    }
}

using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class PermissionsController : BaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public PermissionsController(
            IPermissionService permissionService,
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add([FromBody] PermissionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreatePermission");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreatePermission");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد دسترسی را ندارید.");
            }

            request.CreatedUserId = currentUserId;

            var permission = await _permissionService.AddAsync(request);
            var result = await _permissionService.GetByIdAsync(permission.Id);

            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewPermission");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPermission", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده این دسترسی را ندارید.");
            }

            var permission = await _permissionService.GetByIdAsync(id);
            if (permission == null)
                return NotFound("دسترسی یافت نشد.");
            return Ok(permission);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPermissionList");
            if (!authResult.HasAccess)
                return Forbid();

            // بررسی ورودی‌ها (اختیاری: می‌توانید اعتبارسنجی کنید که PageNumber و PageSize بزرگتر از صفر باشند)
            if (request.PageNumber <= 0)
            {
                return BadRequest("PageNumber must be greater than 0");
            }

            if (request.PageSize <= 0)
            {
                return BadRequest("PageSize must be greater than 0");
            }

            // استفاده از سرویس برای دریافت داده‌ها
            var result = await _permissionService.GetPagedListAsync(request);

            return PaginatedResponse<PermissionDto>(result.Items, request.PageNumber, request.PageSize, result.TotalCount);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] PermissionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditPermission", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این دسترسی را ندارید.");

            var result = await _permissionService.UpdateAsync(request);
            return Ok(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeletePermission");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeletePermission", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این دسترسی را ندارید.");
            }

            await _permissionService.DeleteAsync(id);

            return NoContent();
        }
    }
}

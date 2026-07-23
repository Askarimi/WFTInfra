using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public RolesController(
            IRoleService roleService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _roleService = roleService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] RoleDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreateRole");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreateRole");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد نقش را ندارید.");
            }

            request.CreatedUserId = currentUserId;

            var role = await _roleService.AddAsync(request);
            var result = await _roleService.GetByIdAsync(role.Id);

            return CreatedAtAction(nameof(GetById), new { id = role.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewRole");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewRole");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده نقش را ندارید.");
            }

            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
                return NotFound("نقش یافت نشد.");

            return Ok(role);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewRoleList");
            if (!authResult.HasAccess)
                return Forbid();

            if (request.PageNumber <= 0) return BadRequest("PageNumber must be greater than 0");
            if (request.PageSize <= 0) return BadRequest("PageSize must be greater than 0");

            var result = await _roleService.GetPagedListAsync(request);

            return PaginatedResponse<RoleDto>(result.Items, request.PageNumber, request.PageSize, result.TotalCount);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] RoleDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditRole", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این نقش را ندارید.");

            var result = await _roleService.UpdateAsync(request);

            return Ok(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeleteRole");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeleteRole", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این نقش را ندارید.");
            }

            await _roleService.DeleteAsync(id);

            return NoContent();
        }
    }
}

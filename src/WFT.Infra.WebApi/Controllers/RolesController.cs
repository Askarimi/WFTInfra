using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IWorkContext _workContext;

        public RolesController(IRoleService roleService, IWorkContext workContext)
        {
            _roleService = roleService;
            _workContext = workContext;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add([FromBody] RoleDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            request.CreatedUserId = _workContext.UserId.Value;

            var role = await _roleService.AddAsync(request);
            var result = await _roleService.GetByIdAsync(role.Id);

            return await CreatedResponse(nameof(GetById), new { id = role.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
                return await ErrorResponse("نقش یافت نشد.", 404);

            return await SuccessResponse(role);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List([FromQuery] PagedQueryRequest request)
        {
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
            var result = await _roleService.GetPagedListAsync(request);

            // بازگشت نتیجه صفحه‌بندی شده
            return await SuccessResponse(result);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] RoleDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var result = await _roleService.UpdateAsync(request);

            return await SuccessResponse(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _roleService.DeleteAsync(id);

            return await NoContentResponse();
        }
    }
}

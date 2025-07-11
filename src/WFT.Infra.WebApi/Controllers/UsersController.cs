using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{

    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;
        public UsersController(IUserService userService, IWorkContext workContext)
        {
            _userService = userService;
            _workContext = workContext;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");


            request.CreatedUserId = _workContext.UserId.Value;

            var user = await _userService.AddAsync(request);
            var result = await _userService.GetByIdAsync(user.Id);

            return await CreatedResponse(nameof(GetById), new { id = user.Id }, result);
        }

        // READ BY ID
        [HttpGet()]
        [Route("init/{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return await ErrorResponse("کاربر یافت نشد.", 404);

            return await SuccessResponse(user);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
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
            var result = await _userService.GetPagedListAsync(request);

            // بازگشت نتیجه صفحه‌بندی شده
            return await SuccessResponse(result);
        }

        // UPDATE
        [HttpPut()]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var result = await _userService.UpdateAsync(request);

            return await SuccessResponse(result);
        }

        // DELETE
        [HttpDelete()]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _userService.DeleteAsync(id);

            return await NoContentResponse();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    [Authorize]
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
        [HttpGet("{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return await ErrorResponse("کاربر یافت نشد.", 404);

            return await SuccessResponse(user);
        }

        // READ ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return await SuccessResponse(users);
        }

        // UPDATE
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            await _userService.UpdateAsync(request);

            return await NoContentResponse();
        }

        // DELETE
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _userService.DeleteAsync(id);

            return await NoContentResponse();
        }
    }
}

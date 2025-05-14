using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.WebApi.Controllers
{
    public class AuthController : BaseController
    {

        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public virtual async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                var user= await _userService.RegisterByUserAsync(dto);

                return new OkObjectResult(new {success = true,message = "کاربر با موفقیت ثبت شد."});
            }
            catch (Exception ex)
            {
                return await ErrorResponse(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var result = await _userService.LoginAsync(dto);

                if (result == null)
                    return await ErrorResponse("Invalid username or password", 401);

                return await SuccessResponse(result); // مثلاً توکن، اطلاعات کاربر و...
            }
            catch (Exception ex)
            {
                return await ErrorResponse(ex.Message);
            }
        }
    }
}

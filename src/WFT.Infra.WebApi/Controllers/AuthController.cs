using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {

        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IWorkContext _workContext;

        public AuthController(IUserService userService,
            IAuthService authService,
            ITokenService tokenService,
            IWorkContext workContext
            )
        {
            _userService = userService;
            _authService = authService;
            _tokenService = tokenService;
            _workContext = workContext;
        }

        [HttpPost("register")]
        public virtual async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                var user = await _authService.RegisterAsync(dto);

                return new OkObjectResult(new { success = true, message = "کاربر با موفقیت ثبت شد." });
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
                var result = await _authService.LoginAsync(dto);

                if (result == null)
                    return await ErrorResponse("Invalid username or password", 401);


                // RefreshToken از result بگیریم و داخل Cookie بذاریم
                Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return await SuccessResponse(new { accesstoken = result.AccessToken }); // مثلاً توکن، اطلاعات کاربر و...
            }
            catch (Exception ex)
            {
                return await ErrorResponse(ex.Message);
            }
        }



        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {

            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return await ErrorResponse("توکن یافت نشد", 400);

                await _authService.LogoutAsync(refreshToken); // این متد همونیه که Revoke میکنه

                Response.Cookies.Delete("refreshToken");

                return await SuccessResponse("با موفقیت خارج شدید");
            }
            catch (Exception ex)
            {
                return await ErrorResponse("خطا در خروج: " + ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var newTokens = await _tokenService.RefreshTokenAsync(refreshToken);

            if (newTokens == null)
                return Unauthorized();

            // RefreshToken جدید رو باز هم در Cookie بنویس
            Response.Cookies.Append("refresh_token", newTokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = newTokens.AccessToken });
        }
    }
}

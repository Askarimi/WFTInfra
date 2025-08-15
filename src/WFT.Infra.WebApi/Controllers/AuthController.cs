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

        public AuthController(
            IUserService userService,
            IAuthService authService,
            ITokenService tokenService,
            IWorkContext workContext)
        {
            _userService = userService;
            _authService = authService;
            _tokenService = tokenService;
            _workContext = workContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                var user = await _authService.RegisterAsync(dto);
                return Ok(new { message = "کاربر با موفقیت ثبت شد." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                if (result == null)
                    return Unauthorized("نام کاربری یا رمز عبور معتبر نیست.");

                // RefreshToken در Cookie
                Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Ok(new
                {
                    accessToken = result.AccessToken,
                    user = new
                    {
                        id = result.User.Id,
                        username = result.User.Username,
                        firstname = result.User.FirstName,
                        lastname = result.User.LastName,
                        fullName = $"{result.User.FirstName} {result.User.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies["refresh_token"];
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return BadRequest("توکن یافت نشد");

                await _authService.LogoutAsync(refreshToken);

                Response.Cookies.Delete("refresh_token");

                return Ok("با موفقیت خارج شدید");
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در خروج: " + ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("توکن موجود نیست.");

            var newTokens = await _tokenService.RefreshTokenAsync(refreshToken);
            if (newTokens == null)
                return Unauthorized("توکن نامعتبر است.");

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

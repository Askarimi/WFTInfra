using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;
using WFT.Infra.WebApi.CustomConfig;

namespace WFT.Infra.WebApi.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly ILoginService _loginService;
        private readonly ITokenService _tokenService;
        private readonly IWorkContext _workContext;

        public AuthController(
            IUserService userService,
            IAuthService authService,
            ILoginService loginService,
            ITokenService tokenService,
            IWorkContext workContext)
        {
            _userService = userService;
            _authService = authService;
            _loginService = loginService;
            _tokenService = tokenService;
            _workContext = workContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                var user = await _authService.RegisterAsync(dto);
                return WFTJsonResult.Ok(new { message = "کاربر با موفقیت ثبت شد." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
                    return WFTJsonResult.Fail("نام کاربری و رمز عبور الزامی هستند", 400);

                // Login with roles and permissions
                var result = await _loginService.LoginWithRolesAndPermissionsAsync(dto);
                if (result == null)
                    return WFTJsonResult.Fail("نام کاربری یا رمز عبور معتبر نیست", 401);

                // Set refresh token in secure cookie
                Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                // Return enhanced response with roles and permissions
                return WFTJsonResult.Ok(new
                {
                    accessToken = result.AccessToken,
                    user = new
                    {
                        id = result.User.Id,
                        username = result.User.Username,
                        firstname = result.User.FirstName,
                        lastname = result.User.LastName,
                        email = result.User.Email,
                        fullName = result.User.FullName,
                        isActive = result.User.IsActive
                    },
                    roles = result.Roles,
                    permissions = result.Permissions
                }, message: "ورود موفقیت‌آمیز");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("خطای داخلی سرور", 500, ex);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies["refresh_token"];
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return WFTJsonResult.Fail("توکن یافت نشد", 400);

                await _authService.LogoutAsync(refreshToken);
                Response.Cookies.Delete("refresh_token");

                return WFTJsonResult.Ok(message: "با موفقیت خارج شدید");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("خطا در خروج", 500, ex);
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refresh_token"];
                if (string.IsNullOrEmpty(refreshToken))
                    return WFTJsonResult.Fail("توکن موجود نیست", 401);

                var newTokens = await _tokenService.RefreshTokenAsync(refreshToken);
                if (newTokens == null)
                    return WFTJsonResult.Fail("توکن نامعتبر است", 401);

                Response.Cookies.Append("refresh_token", newTokens.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return WFTJsonResult.Ok(new { accessToken = newTokens.AccessToken });
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("خطا در تازه‌سازی توکن", 500, ex);
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class UserPasswordController : BaseController
    {
        private readonly IUserPasswordService _userPasswordService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public UserPasswordController(
            IUserPasswordService userPasswordService,
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _userPasswordService = userPasswordService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // POST /api/userpassword/set
        [HttpPost]
        [Route("set")]
        public async Task<IActionResult> SetPassword([FromBody] UserPasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            // Validation
            if (string.IsNullOrEmpty(request.Password))
                return BadRequest("رمز عبور الزامی است.");

            if (string.IsNullOrEmpty(request.ConfirmPassword))
                return BadRequest("تأیید رمز عبور الزامی است.");

            if (request.Password != request.ConfirmPassword)
                return BadRequest("رمز عبور و تأیید آن باید یکسان باشند.");

            var currentUserId = _workContext.UserId!.Value;

            //var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "SetUserPassword");
            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "SetUserPassword");
            if (!authResult.HasAccess)
            {
                return StatusCode(StatusCodes.Status403Forbidden, authResult.EvaluationReason);
            }

            try
            {
                var result = await _userPasswordService.SetPasswordAsync(request);
                return StatusCode(201, new { Success = true, Data = new { Message = "رمز عبور با موفقیت تنظیم شد." } });
            }
            catch (Exception ex)
            {
                return BadRequest($"خطا در تنظیم رمز عبور: {ex.Message}");
            }
        }

        // PUT /api/userpassword/update
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> ChangePassword([FromBody] UserPasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            // Validation
            if (string.IsNullOrEmpty(request.Password))
                return BadRequest("رمز عبور الزامی است.");

            if (string.IsNullOrEmpty(request.ConfirmPassword))
                return BadRequest("تأیید رمز عبور الزامی است.");

            if (request.Password != request.ConfirmPassword)
                return BadRequest("رمز عبور و تأیید آن باید یکسان باشند.");

            var currentUserId = _workContext.UserId!.Value;

            // var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ChangeUserPassword");

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ChangeUserPassword");

            if (!authResult.HasAccess)
            {
                return StatusCode(StatusCodes.Status403Forbidden, authResult.EvaluationReason);
            }

            try
            {
                var result = await _userPasswordService.ChangePasswordAsync(request);
                return Ok(new { Message = "رمز عبور با موفقیت تغییر یافت." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"خطا در تغییر رمز عبور: {ex.Message}");
            }
        }
    }
}
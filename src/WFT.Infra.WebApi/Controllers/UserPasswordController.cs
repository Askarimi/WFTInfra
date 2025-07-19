using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;

namespace WFT.Infra.WebApi.Controllers
{
    public class UserPasswordController : BaseController
    {
        private readonly IUserPasswordService _userPasswordService;

        public UserPasswordController(IUserPasswordService userPasswordService)
        {
            _userPasswordService = userPasswordService;
        }

        // POST /api/userpassword/set
        [HttpPost]
        [Route("set")]
        public async Task<IActionResult> SetPassword([FromBody] UserPasswordDto request)
        {
            // Validation
            if (string.IsNullOrEmpty(request.Password))
                return await ErrorResponse("Password is required.", 400);

            if (string.IsNullOrEmpty(request.ConfirmPassword))
                return await ErrorResponse("Confirm password is required.", 400);

            if (request.Password != request.ConfirmPassword)
                return await ErrorResponse("Password and confirm password must match.", 400);

            if (!ModelState.IsValid)
                return await ErrorResponse("Invalid data provided.", 400);

            try
            {
                var result = await _userPasswordService.SetPasswordAsync(request);
                return StatusCode(201, new { Success = true, Data = new { Message = "Password set successfully." } });
            }
            catch (Exception ex)
            {
                return await ErrorResponse($"Failed to set password: {ex.Message}", 500);
            }
        }

        // PUT /api/userpassword/update
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> ChangePassword([FromBody] UserPasswordDto request)
        {
            // Validation
            if (string.IsNullOrEmpty(request.Password))
                return await ErrorResponse("Password is required.", 400);

            if (string.IsNullOrEmpty(request.ConfirmPassword))
                return await ErrorResponse("Confirm password is required.", 400);

            if (request.Password != request.ConfirmPassword)
                return await ErrorResponse("Password and confirm password must match.", 400);

            if (!ModelState.IsValid)
                return await ErrorResponse("Invalid data provided.", 400);

            try
            {
                var result = await _userPasswordService.ChangePasswordAsync(request);
                return await SuccessResponse(new { Message = "Password changed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return await ErrorResponse(ex.Message, 400);
            }
            catch (Exception ex)
            {
                return await ErrorResponse($"Failed to change password: {ex.Message}", 500);
            }
        }
    }
} 
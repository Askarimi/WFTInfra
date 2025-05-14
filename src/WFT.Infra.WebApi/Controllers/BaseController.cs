using Microsoft.AspNetCore.Mvc;

namespace WFT.Infra.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {

        // متد برای پاسخ موفق
        protected virtual async Task<IActionResult> SuccessResponse(object result)
        {
            return Ok(new { Success = true, Data = result });
        }

        // متد برای پاسخ خطا
        protected virtual async Task<IActionResult> ErrorResponse(string errorMessage, int statusCode = 400)
        {
            return StatusCode(statusCode, new { Success = false, Error = errorMessage });
        }

        // متد برای پاسخ ایجاد (برای 201 Created)
        protected virtual async Task<IActionResult> CreatedResponse(string actionName, object routeValues, object result)
        {
            return CreatedAtAction(actionName, routeValues, new { Success = true, Data = result });
        }

        // متد برای پاسخ 204 (No Content)
        protected async Task<IActionResult> NoContentResponse()
        {
            return NoContent();
        }

        // فرض بر این است که متد GetById در کنترلر مربوطه تعریف شده باشد
        [HttpGet("id")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            return Ok(new { Success = true, Message = "GetById method should be overridden" });
        }
    }
}

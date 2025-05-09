using Microsoft.AspNetCore.Mvc;

namespace WFT.Infra.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {

        // متد برای پاسخ موفق
        protected IActionResult SuccessResponse(object result)
        {
            return Ok(new { Success = true, Data = result });
        }

        // متد برای پاسخ خطا
        protected IActionResult ErrorResponse(string errorMessage, int statusCode = 400)
        {
            return StatusCode(statusCode, new { Success = false, Error = errorMessage });
        }

        // متد برای پاسخ ایجاد (برای 201 Created)
        protected IActionResult CreatedResponse(object result)
        {
            return CreatedAtAction(nameof(GetById), new { id = 1 }, result);  // اینجا باید آی‌دی و مسیر مناسب را اضافه کنید
        }

        // متد برای پاسخ 204 (No Content)
        protected IActionResult NoContentResponse()
        {
            return NoContent();
        }

        // فرض بر این است که متد GetById در کنترلر مربوطه تعریف شده باشد
        [HttpGet("id")]
        public virtual IActionResult GetById(int id)
        {
            return Ok(new { Success = true, Message = "GetById method should be overridden" });
        }
    }
}

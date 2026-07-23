using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.WebApi.CustomConfig;

namespace WFT.Infra.WebApi.Controllers
{
    [Authorize]
    [Route("api/app/v1/wft/[controller]", Name = "api_wftInfra_[controller]", Order = 0)]
    [ApiController]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Returns a standardized paginated response.
        /// The actual JSON envelope wrapping ({ isSuccess, statusCode, data, meta }) 
        /// is automatically applied globally by WFTResponseMiddleware.
        /// </summary>
        protected IActionResult PaginatedResponse<T>(IEnumerable<T> data, int page, int pageSize, int totalItems, string? message = null)
        {
            // محاسبه اطلاعات صفحه‌بندی در قالب آبجکت Meta
            var meta = new
            {
                pageNumber = page,
                pageSize = pageSize,
                totalCount = totalItems,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                hasNextPage = page * pageSize < totalItems,
                hasPreviousPage = page > 1
            };

            // بازگرداندن مستقیم PagedResponse. 
            // Middleware این فرمت را شناسایی کرده و ساختار نهایی را می‌سازد.
            return Ok(new PagedResponse<T>(data, meta));
        }
    }
}

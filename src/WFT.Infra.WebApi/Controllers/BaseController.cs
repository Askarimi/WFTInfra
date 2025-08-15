using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.WebApi.CustomConfig;

namespace WFT.Infra.WebApi.Controllers
{
    [Authorize]
    [Route("api/app/v1/wft/[controller]", Name = "api_wftInfra_[controller]", Order = 0)]
    [ApiController]
    public class BaseController : ControllerBase
    {


        // متد صفحه‌بندی برای راحتی
        protected IActionResult PaginatedResponse<T>(IEnumerable<T> data, int page, int pageSize, int totalItems, List<string>? warnings = null)
        {
            var meta = new MetaData
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Warnings = warnings
            };

            return Ok(new { Data = data, Meta = meta });
        }
    }
}

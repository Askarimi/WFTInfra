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
        /// Returns a standardized paginated response following JSON:API conventions
        /// Response structure: { success, data: [], meta: { page, pageSize, totalItems, totalPages } }
        /// Serialized to camelCase format
        /// Uses ApiEnvelope for consistent root-level structure
        /// </summary>
        /// <typeparam name="T">The type of items in the paged result</typeparam>
        /// <param name="data">The collection of items for the current page</param>
        /// <param name="page">Current page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="totalItems">Total number of items across all pages</param>
        /// <param name="message">Optional success message</param>
        /// <returns>IActionResult with ApiEnvelope wrapping flat data array and meta object</returns>
        protected IActionResult PaginatedResponse<T>(IEnumerable<T> data, int page, int pageSize, int totalItems, string? message = null)
        {
            // Use PagedResponse internally to compute Meta
            var paged = PagedResponse<T>.Ok(data, page, pageSize, totalItems, message);
            
            // Wrap in ApiEnvelope for consistent root-level structure
            var envelope = ApiEnvelope<IEnumerable<T>>.Ok(paged.Data, paged.Message, paged.Meta);
            
            return Ok(envelope);
        }
    }
}

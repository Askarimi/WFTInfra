using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class CountryController : BaseController
    {
        private readonly ICountryService _countryService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public CountryController(
            ICountryService countryService,
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _countryService = countryService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] CountryDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreateCountry");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreateCountry");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد کشور را ندارید.");
            }

            request.CreatedUserId = currentUserId;

            var country = await _countryService.AddAsync(request);
            var result = await _countryService.GetByIdAsync(country.Id);

            return CreatedAtAction(nameof(GetById), new { id = country.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewCountry");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewCountry");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده کشور را ندارید.");
            }

            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
                return NotFound("کشور یافت نشد.");

            return Ok(country);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewCountryList");
            if (!authResult.HasAccess)
                return Forbid();

            if (request.PageNumber <= 0) return BadRequest("PageNumber must be greater than 0");
            if (request.PageSize <= 0) return BadRequest("PageSize must be greater than 0");

            var result = await _countryService.GetPagedListAsync(request);

            return PaginatedResponse<CountryDto>(result.Items, request.PageNumber, request.PageSize, result.TotalCount);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] CountryDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditCountry", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این کشور را ندارید.");

            var result = await _countryService.UpdateAsync(request);

            return Ok(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeleteCountry");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeleteCountry", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این کشور را ندارید.");
            }

            await _countryService.DeleteAsync(id);

            return NoContent();
        }
    }
}

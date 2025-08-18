using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class AttributeDefinitionsController : BaseController
    {
        private readonly IAttributeService _attributeService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public AttributeDefinitionsController(
            IAttributeService attributeService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _attributeService = attributeService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] AttributeDefinitionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreateAttributeDefinition");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreateAttributeDefinition");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد تعریف ویژگی را ندارید.");
            }

            request.CreatedUserId = currentUserId;

            var attributeDefinition = await _attributeService.AddAsync(request);
            var result = await _attributeService.GetByIdAsync(attributeDefinition.Id);

            return CreatedAtAction(nameof(GetById), new { id = attributeDefinition.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewAttributeDefinition");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeDefinition");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده تعریف ویژگی را ندارید.");
            }

            var attributeDefinition = await _attributeService.GetByIdAsync(id);
            if (attributeDefinition == null)
                return NotFound("تعریف ویژگی یافت نشد.");

            return Ok(attributeDefinition);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeDefinitionList");
            if (!authResult.HasAccess)
                return Forbid();

            if (request.PageNumber <= 0) return BadRequest("PageNumber must be greater than 0");
            if (request.PageSize <= 0) return BadRequest("PageSize must be greater than 0");

            var result = await _attributeService.GetPagedListAsync(request);

            return PaginatedResponse<AttributeDefinitionDto>(result.Items, request.PageNumber, request.PageSize, result.TotalCount);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] AttributeDefinitionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditAttributeDefinition", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این تعریف ویژگی را ندارید.");

            var result = await _attributeService.UpdateAsync(request);

            return Ok(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeleteAttributeDefinition");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeleteAttributeDefinition", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این تعریف ویژگی را ندارید.");
            }

            await _attributeService.DeleteAsync(id);

            return NoContent();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewAttributeDefinition");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeDefinition");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده تعریف ویژگی را ندارید.");
            }

            var attributeDefinition = await _attributeService.GetByNameAsync(name);
            if (attributeDefinition == null)
                return NotFound("تعریف ویژگی یافت نشد.");

            return Ok(attributeDefinition);
        }

        // Get by source
        [HttpGet]
        [Route("bysource/{source}")]
        public async Task<IActionResult> GetBySource(string source)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewAttributeDefinition");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeDefinition");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده تعریف ویژگی را ندارید.");
            }

            var attributeDefinitions = await _attributeService.GetAttributeDefinitionsBySourceAsync(source);

            return Ok(attributeDefinitions);
        }
    }
} 
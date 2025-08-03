using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class AttributeDefinitionsController : BaseController
    {
        private readonly IAttributeService _attributeService;
        private readonly IWorkContext _workContext;

        public AttributeDefinitionsController(IAttributeService attributeService, IWorkContext workContext)
        {
            _attributeService = attributeService;
            _workContext = workContext;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] AttributeDefinitionDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            request.CreatedUserId = _workContext.UserId.Value;

            var attributeDefinition = await _attributeService.AddAsync(request);
            var result = await _attributeService.GetByIdAsync(attributeDefinition.Id);

            return await CreatedResponse(nameof(GetById), new { id = attributeDefinition.Id }, result);
        }

        // READ BY ID
        [HttpGet()]
        [Route("init/{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            var attributeDefinition = await _attributeService.GetByIdAsync(id);
            if (attributeDefinition == null)
                return await ErrorResponse("تعریف ویژگی یافت نشد.", 404);

            return await SuccessResponse(attributeDefinition);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            if (request.PageNumber <= 0)
            {
                return BadRequest("PageNumber must be greater than 0");
            }

            if (request.PageSize <= 0)
            {
                return BadRequest("PageSize must be greater than 0");
            }

            var result = await _attributeService.GetPagedListAsync(request);

            return await SuccessResponse(result);
        }

        // UPDATE
        [HttpPut()]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] AttributeDefinitionDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var result = await _attributeService.UpdateAsync(request);

            return await SuccessResponse(result);
        }

        // DELETE
        [HttpDelete()]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _attributeService.DeleteAsync(id);

            return await NoContentResponse();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var attributeDefinition = await _attributeService.GetByNameAsync(name);
            if (attributeDefinition == null)
                return await ErrorResponse("تعریف ویژگی یافت نشد.", 404);

            return await SuccessResponse(attributeDefinition);
        }

        // Get by source
        [HttpGet]
        [Route("bysource/{source}")]
        public async Task<IActionResult> GetBySource(string source)
        {
            var attributeDefinitions = await _attributeService.GetAttributeDefinitionsBySourceAsync(source);

            return await SuccessResponse(attributeDefinitions);
        }
    }
} 
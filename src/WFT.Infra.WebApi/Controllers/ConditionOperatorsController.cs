using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class ConditionOperatorsController : BaseController
    {
        private readonly IConditionOperatorService _conditionOperatorService;
        private readonly IWorkContext _workContext;

        public ConditionOperatorsController(IConditionOperatorService conditionOperatorService, IWorkContext workContext)
        {
            _conditionOperatorService = conditionOperatorService;
            _workContext = workContext;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] ConditionOperatorDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            request.CreatedUserId = _workContext.UserId.Value;

            var conditionOperator = await _conditionOperatorService.AddAsync(request);
            var result = await _conditionOperatorService.GetByIdAsync(conditionOperator.Id);

            return await CreatedResponse(nameof(GetById), new { id = conditionOperator.Id }, result);
        }

        // READ BY ID
        [HttpGet()]
        [Route("init/{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            var conditionOperator = await _conditionOperatorService.GetByIdAsync(id);
            if (conditionOperator == null)
                return await ErrorResponse("عملگر شرط یافت نشد.", 404);

            return await SuccessResponse(conditionOperator);
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

            var result = await _conditionOperatorService.GetPagedListAsync(request);

            return await SuccessResponse(result);
        }

        // UPDATE
        [HttpPut()]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] ConditionOperatorDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var result = await _conditionOperatorService.UpdateAsync(request);

            return await SuccessResponse(result);
        }

        // DELETE
        [HttpDelete()]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _conditionOperatorService.DeleteAsync(id);

            return await NoContentResponse();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var conditionOperator = await _conditionOperatorService.GetByNameAsync(name);
            if (conditionOperator == null)
                return await ErrorResponse("عملگر شرط یافت نشد.", 404);

            return await SuccessResponse(conditionOperator);
        }

        // Get active operators
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActiveOperators()
        {
            var operators = await _conditionOperatorService.GetActiveOperatorsAsync();

            return await SuccessResponse(operators);
        }

        // Get operators by data type
        [HttpGet]
        [Route("bydatatype/{dataType}")]
        public async Task<IActionResult> GetOperatorsByDataType(string dataType)
        {
            var operators = await _conditionOperatorService.GetOperatorsByDataTypeAsync(dataType);

            return await SuccessResponse(operators);
        }
    }
} 
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class ConditionOperatorsController : BaseController
    {
        private readonly IConditionOperatorService _conditionOperatorService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public ConditionOperatorsController(
            IConditionOperatorService conditionOperatorService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _conditionOperatorService = conditionOperatorService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] ConditionOperatorDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreateConditionOperator");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreateConditionOperator");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد عملگر شرط را ندارید.");
            }

            request.CreatedUserId = currentUserId;

            var conditionOperator = await _conditionOperatorService.AddAsync(request);
            var result = await _conditionOperatorService.GetByIdAsync(conditionOperator.Id);

            return CreatedAtAction(nameof(GetById), new { id = conditionOperator.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewConditionOperator");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewConditionOperator");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده عملگر شرط را ندارید.");
            }

            var conditionOperator = await _conditionOperatorService.GetByIdAsync(id);
            if (conditionOperator == null)
                return NotFound("عملگر شرط یافت نشد.");

            return Ok(conditionOperator);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewConditionOperatorList");
            if (!authResult.HasAccess)
                return Forbid();

            if (request.PageNumber <= 0) return BadRequest("PageNumber must be greater than 0");
            if (request.PageSize <= 0) return BadRequest("PageSize must be greater than 0");

            var result = await _conditionOperatorService.GetPagedListAsync(request);

            return PaginatedResponse<ConditionOperatorDto>(result.Items, request.PageNumber, request.PageSize, result.TotalCount);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] ConditionOperatorDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditConditionOperator", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این عملگر شرط را ندارید.");

            var result = await _conditionOperatorService.UpdateAsync(request);

            return Ok(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeleteConditionOperator");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeleteConditionOperator", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این عملگر شرط را ندارید.");
            }

            await _conditionOperatorService.DeleteAsync(id);

            return NoContent();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewConditionOperator");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewConditionOperator");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده عملگر شرط را ندارید.");
            }

            var conditionOperator = await _conditionOperatorService.GetByNameAsync(name);
            if (conditionOperator == null)
                return NotFound("عملگر شرط یافت نشد.");

            return Ok(conditionOperator);
        }

        // Get active operators
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActiveOperators()
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewConditionOperator");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewConditionOperator");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده عملگر شرط را ندارید.");
            }

            var operators = await _conditionOperatorService.GetActiveOperatorsAsync();

            return Ok(operators);
        }

        // Get operators by data type
        [HttpGet]
        [Route("bydatatype/{dataType}")]
        public async Task<IActionResult> GetOperatorsByDataType(string dataType)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewConditionOperator");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewConditionOperator");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده عملگر شرط را ندارید.");
            }

            var operators = await _conditionOperatorService.GetOperatorsByDataTypeAsync(dataType);

            return Ok(operators);
        }
    }
} 
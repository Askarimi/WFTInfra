using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class PolicyRulesController : BaseController
    {
        private readonly IPolicyRuleService _policyRuleService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public PolicyRulesController(
            IPolicyRuleService policyRuleService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _policyRuleService = policyRuleService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] PolicyRuleDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreatePolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreatePolicyRule");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد قانون سیاست را ندارید.");
            }

            request.CreatedUserId = currentUserId;

            var policyRule = await _policyRuleService.AddAsync(request);
            var result = await _policyRuleService.GetByIdAsync(policyRule.Id);

            return CreatedAtAction(nameof(GetById), new { id = policyRule.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewPolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPolicyRule");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده قانون سیاست را ندارید.");
            }

            var policyRule = await _policyRuleService.GetByIdAsync(id);
            if (policyRule == null)
                return NotFound("قانون سیاست یافت نشد.");

            return Ok(policyRule);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPolicyRuleList");
            if (!authResult.HasAccess)
                return Forbid();

            if (request.PageNumber <= 0) return BadRequest("PageNumber must be greater than 0");
            if (request.PageSize <= 0) return BadRequest("PageSize must be greater than 0");

            var result = await _policyRuleService.GetPagedListAsync(request);

            return PaginatedResponse<PolicyRuleDto>(result.Items, request.PageNumber, request.PageSize, result.TotalCount);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] PolicyRuleDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditPolicyRule", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این قانون سیاست را ندارید.");

            var result = await _policyRuleService.UpdateAsync(request);

            return Ok(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeletePolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeletePolicyRule", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این قانون سیاست را ندارید.");
            }

            await _policyRuleService.DeleteAsync(id);

            return NoContent();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewPolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPolicyRule");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده قانون سیاست را ندارید.");
            }

            var policyRule = await _policyRuleService.GetByNameAsync(name);
            if (policyRule == null)
                return NotFound("قانون سیاست یافت نشد.");

            return Ok(policyRule);
        }

        // Get policies for role
        [HttpGet]
        [Route("forrole/{roleId:long}")]
        public async Task<IActionResult> GetPoliciesForRole(long roleId)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewPolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPolicyRule");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده قانون سیاست را ندارید.");
            }

            var policies = await _policyRuleService.GetPoliciesForRoleAsync(roleId);

            return Ok(policies);
        }

        // Get policies for permission
        [HttpGet]
        [Route("forpermission/{permission}")]
        public async Task<IActionResult> GetPoliciesForPermission(string permission)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewPolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPolicyRule");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده قانون سیاست را ندارید.");
            }

            var policies = await _policyRuleService.GetPoliciesForPermissionAsync(permission);

            return Ok(policies);
        }

        // Get active policies
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActivePolicies()
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewPolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPolicyRule");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده قانون سیاست را ندارید.");
            }

            var policies = await _policyRuleService.GetActivePoliciesAsync();

            return Ok(policies);
        }
    }
} 
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class PolicyRulesController : BaseController
    {
        private readonly IPolicyRuleService _policyRuleService;
        private readonly IWorkContext _workContext;

        public PolicyRulesController(IPolicyRuleService policyRuleService, IWorkContext workContext)
        {
            _policyRuleService = policyRuleService;
            _workContext = workContext;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] PolicyRuleDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            request.CreatedUserId = _workContext.UserId.Value;

            var policyRule = await _policyRuleService.AddAsync(request);
            var result = await _policyRuleService.GetByIdAsync(policyRule.Id);

            return await CreatedResponse(nameof(GetById), new { id = policyRule.Id }, result);
        }

        // READ BY ID
        [HttpGet()]
        [Route("init/{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            var policyRule = await _policyRuleService.GetByIdAsync(id);
            if (policyRule == null)
                return await ErrorResponse("قانون سیاست یافت نشد.", 404);

            return await SuccessResponse(policyRule);
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

            var result = await _policyRuleService.GetPagedListAsync(request);

            return await SuccessResponse(result);
        }

        // UPDATE
        [HttpPut()]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] PolicyRuleDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var result = await _policyRuleService.UpdateAsync(request);

            return await SuccessResponse(result);
        }

        // DELETE
        [HttpDelete()]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _policyRuleService.DeleteAsync(id);

            return await NoContentResponse();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var policyRule = await _policyRuleService.GetByNameAsync(name);
            if (policyRule == null)
                return await ErrorResponse("قانون سیاست یافت نشد.", 404);

            return await SuccessResponse(policyRule);
        }

        // Get policies for role
        [HttpGet]
        [Route("forrole/{roleId:long}")]
        public async Task<IActionResult> GetPoliciesForRole(long roleId)
        {
            var policies = await _policyRuleService.GetPoliciesForRoleAsync(roleId);

            return await SuccessResponse(policies);
        }

        // Get policies for permission
        [HttpGet]
        [Route("forpermission/{permission}")]
        public async Task<IActionResult> GetPoliciesForPermission(string permission)
        {
            var policies = await _policyRuleService.GetPoliciesForPermissionAsync(permission);

            return await SuccessResponse(policies);
        }

        // Get active policies
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActivePolicies()
        {
            var policies = await _policyRuleService.GetActivePoliciesAsync();

            return await SuccessResponse(policies);
        }
    }
} 
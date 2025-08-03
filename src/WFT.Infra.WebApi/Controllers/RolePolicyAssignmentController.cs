using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class RolePolicyAssignmentController : BaseController
    {
        private readonly IPolicyRuleService _policyRuleService;
        private readonly IWorkContext _workContext;

        public RolePolicyAssignmentController(IPolicyRuleService policyRuleService, IWorkContext workContext)
        {
            _policyRuleService = policyRuleService;
            _workContext = workContext;
        }

        // Assign policy to role
        [HttpPost]
        [Route("assign/{roleId:long}/{policyRuleId:long}")]
        public async Task<IActionResult> AssignPolicyToRole(long roleId, long policyRuleId)
        {
            var success = await _policyRuleService.AddPolicyToRoleAsync(roleId, policyRuleId);
            if (!success)
                return await ErrorResponse("خطا در تخصیص سیاست به نقش.");

            return await SuccessResponse(new { Success = true, Message = "سیاست با موفقیت به نقش تخصیص داده شد." });
        }

        // Remove policy from role
        [HttpDelete]
        [Route("remove/{roleId:long}/{policyRuleId:long}")]
        public async Task<IActionResult> RemovePolicyFromRole(long roleId, long policyRuleId)
        {
            var success = await _policyRuleService.RemovePolicyFromRoleAsync(roleId, policyRuleId);
            if (!success)
                return await ErrorResponse("خطا در حذف سیاست از نقش.");

            return await NoContentResponse();
        }

        // Get policies for role
        [HttpGet]
        [Route("role/{roleId:long}")]
        public async Task<IActionResult> GetPoliciesForRole(long roleId)
        {
            var policies = await _policyRuleService.GetPoliciesForRoleAsync(roleId);

            return await SuccessResponse(policies);
        }

        // Bulk assign policies to role
        [HttpPost]
        [Route("bulkassign/{roleId:long}")]
        public async Task<IActionResult> BulkAssignPoliciesToRole(long roleId, [FromBody] long[] policyRuleIds)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var results = new List<object>();
            foreach (var policyRuleId in policyRuleIds)
            {
                var success = await _policyRuleService.AddPolicyToRoleAsync(roleId, policyRuleId);
                results.Add(new { PolicyRuleId = policyRuleId, Success = success });
            }

            return await SuccessResponse(results);
        }

        // Bulk remove policies from role
        [HttpDelete]
        [Route("bulkremove/{roleId:long}")]
        public async Task<IActionResult> BulkRemovePoliciesFromRole(long roleId, [FromBody] long[] policyRuleIds)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var results = new List<object>();
            foreach (var policyRuleId in policyRuleIds)
            {
                var success = await _policyRuleService.RemovePolicyFromRoleAsync(roleId, policyRuleId);
                results.Add(new { PolicyRuleId = policyRuleId, Success = success });
            }

            return await SuccessResponse(results);
        }
    }
} 
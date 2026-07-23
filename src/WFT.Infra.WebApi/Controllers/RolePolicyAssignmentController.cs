using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class RolePolicyAssignmentController : BaseController
    {
        private readonly IPolicyRuleService _policyRuleService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public RolePolicyAssignmentController(
            IPolicyRuleService policyRuleService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _policyRuleService = policyRuleService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // Assign policy to role
        [HttpPost]
        [Route("assign/{roleId:long}/{policyRuleId:long}")]
        public async Task<IActionResult> AssignPolicyToRole(long roleId, long policyRuleId)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "AssignPolicyToRole");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "AssignPolicyToRole");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز تخصیص سیاست به نقش را ندارید.");
            }

            var success = await _policyRuleService.AddPolicyToRoleAsync(roleId, policyRuleId);
            if (!success)
                return BadRequest("خطا در تخصیص سیاست به نقش.");

            return Ok(new { Success = true, Message = "سیاست با موفقیت به نقش تخصیص داده شد." });
        }

        // Remove policy from role
        [HttpDelete]
        [Route("remove/{roleId:long}/{policyRuleId:long}")]
        public async Task<IActionResult> RemovePolicyFromRole(long roleId, long policyRuleId)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "RemovePolicyFromRole");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "RemovePolicyFromRole");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف سیاست از نقش را ندارید.");
            }

            var success = await _policyRuleService.RemovePolicyFromRoleAsync(roleId, policyRuleId);
            if (!success)
                return BadRequest("خطا در حذف سیاست از نقش.");

            return NoContent();
        }

        // Get policies for role
        [HttpGet]
        [Route("role/{roleId:long}")]
        public async Task<IActionResult> GetPoliciesForRole(long roleId)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewRolePolicyAssignment");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewRolePolicyAssignment");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده تخصیص سیاست به نقش را ندارید.");
            }

            var policies = await _policyRuleService.GetPoliciesForRoleAsync(roleId);

            return Ok(policies);
        }

        // Bulk assign policies to role
        [HttpPost]
        [Route("bulkassign/{roleId:long}")]
        public async Task<IActionResult> BulkAssignPoliciesToRole(long roleId, [FromBody] long[] policyRuleIds)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "AssignPolicyToRole");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "AssignPolicyToRole");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز تخصیص سیاست به نقش را ندارید.");
            }

            var results = new List<object>();
            foreach (var policyRuleId in policyRuleIds)
            {
                var success = await _policyRuleService.AddPolicyToRoleAsync(roleId, policyRuleId);
                results.Add(new { PolicyRuleId = policyRuleId, Success = success });
            }

            return Ok(results);
        }

        // Bulk remove policies from role
        [HttpDelete]
        [Route("bulkremove/{roleId:long}")]
        public async Task<IActionResult> BulkRemovePoliciesFromRole(long roleId, [FromBody] long[] policyRuleIds)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "RemovePolicyFromRole");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "RemovePolicyFromRole");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف سیاست از نقش را ندارید.");
            }

            var results = new List<object>();
            foreach (var policyRuleId in policyRuleIds)
            {
                var success = await _policyRuleService.RemovePolicyFromRoleAsync(roleId, policyRuleId);
                results.Add(new { PolicyRuleId = policyRuleId, Success = success });
            }

            return Ok(results);
        }
    }
} 
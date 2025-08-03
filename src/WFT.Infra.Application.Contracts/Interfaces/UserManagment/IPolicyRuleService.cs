using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IPolicyRuleService : IServiceBase<PolicyRuleDto>
    {
        Task<IEnumerable<PolicyRuleDto>> GetPoliciesForRoleAsync(long roleId);
        Task<IEnumerable<PolicyRuleDto>> GetPoliciesForPermissionAsync(string permission);
        Task<bool> AddPolicyToRoleAsync(long roleId, long policyRuleId);
        Task<bool> RemovePolicyFromRoleAsync(long roleId, long policyRuleId);
        Task<IEnumerable<PolicyRuleDto>> GetActivePoliciesAsync();
    }
} 
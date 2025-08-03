using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IABACService
    {
        Task<bool> EvaluateAccessAsync(long userId, string permission, object? resource = null, object? context = null);
        Task<ABACEvaluationResponseDto> EvaluateAccessDetailedAsync(long userId, string permission, object? resource = null, object? context = null);
        Task<IEnumerable<PolicyRuleDto>> GetApplicablePoliciesAsync(long userId, string permission);
        Task<AttributeValueDto?> GetAttributeValueAsync(long userId, string attributeName);
        Task<IEnumerable<AttributeValueDto>> GetUserAttributesAsync(long userId);
        Task<bool> ValidatePolicyRuleAsync(PolicyRuleDto policyRule, object? resource, object? context);
    }
} 
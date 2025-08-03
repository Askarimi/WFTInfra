using System.Text.Json;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public class ABACService : IABACService
    {
        private readonly IUserService _userService;
        private readonly IPolicyRuleService _policyRuleService;
        private readonly IAttributeService _attributeService;
        private readonly IConditionOperatorService _conditionOperatorService;

        public ABACService(
            IUserService userService,
            IPolicyRuleService policyRuleService,
            IAttributeService attributeService,
            IConditionOperatorService conditionOperatorService)
        {
            _userService = userService;
            _policyRuleService = policyRuleService;
            _attributeService = attributeService;
            _conditionOperatorService = conditionOperatorService;
        }

        public async Task<bool> EvaluateAccessAsync(long userId, string permission, object? resource = null, object? context = null)
        {
            // 1. Check basic RBAC permission first
            var hasBasicPermission = await _userService.HasPermissionAsync(userId, permission);
            if (!hasBasicPermission) return false;

            // 2. Get applicable ABAC policies
            var policies = await GetApplicablePoliciesAsync(userId, permission);
            if (!policies.Any()) return true; // No ABAC rules, allow access

            // 3. Evaluate each policy in priority order
            foreach (var policy in policies.OrderBy(p => p.Priority))
            {
                var policyResult = await ValidatePolicyRuleAsync(policy, resource, context);
                
                if (policy.Effect == "Deny" && policyResult) return false;
                if (policy.Effect == "Allow" && policyResult) return true;
            }

            return false; // Default deny
        }

        public async Task<ABACEvaluationResponseDto> EvaluateAccessDetailedAsync(long userId, string permission, object? resource = null, object? context = null)
        {
            var response = new ABACEvaluationResponseDto
            {
                UserId = userId,
                Permission = permission,
                Resource = resource,
                Context = context,
                EvaluatedAt = DateTime.UtcNow
            };

            // 1. Get user attributes
            response.UserAttributes = (await GetUserAttributesAsync(userId)).ToList();

            // 2. Get resource attributes if resource is provided
            if (resource != null)
            {
                var resourceId = ExtractResourceId(resource);
                if (resourceId.HasValue)
                {
                    response.ResourceAttributes = (await _attributeService.GetResourceAttributesAsync(resourceId.Value, resource.GetType().Name)).ToList();
                }
            }

            // 3. Check basic RBAC permission first
            var hasBasicPermission = await _userService.HasPermissionAsync(userId, permission);
            if (!hasBasicPermission)
            {
                response.HasAccess = false;
                response.EvaluationReason = "کاربر دسترسی پایه RBAC ندارد";
                return response;
            }

            // 4. Get applicable ABAC policies
            var policies = await GetApplicablePoliciesAsync(userId, permission);
            if (!policies.Any())
            {
                response.HasAccess = true;
                response.EvaluationReason = "هیچ قانون ABAC اعمال نمی‌شود، دسترسی مجاز است";
                return response;
            }

            // 5. Evaluate each policy in priority order
            foreach (var policy in policies.OrderBy(p => p.Priority))
            {
                var policyResult = await EvaluatePolicyDetailedAsync(policy, resource, context, userId);
                response.PolicyResults.Add(policyResult);

                if (policy.Effect == "Deny" && policyResult.ConditionsMet)
                {
                    response.HasAccess = false;
                    response.EvaluationReason = $"دسترسی توسط قانون '{policy.Name}' رد شد";
                    return response;
                }

                if (policy.Effect == "Allow" && policyResult.ConditionsMet)
                {
                    response.HasAccess = true;
                    response.EvaluationReason = $"دسترسی توسط قانون '{policy.Name}' مجاز شد";
                    return response;
                }
            }

            // 6. Default deny
            response.HasAccess = false;
            response.EvaluationReason = "هیچ قانونی شرایط را برآورده نمی‌کند، دسترسی رد شد";
            return response;
        }

        public async Task<IEnumerable<PolicyRuleDto>> GetApplicablePoliciesAsync(long userId, string permission)
        {
            // Get user's roles
            var userRoles = await _userService.GetRolesForUserAsync(userId);
            var roleIds = userRoles.Select(r => r.Id).ToList();

            // Get policies for all user roles
            var allPolicies = new List<PolicyRuleDto>();
            foreach (var roleId in roleIds)
            {
                var rolePolicies = await _policyRuleService.GetPoliciesForRoleAsync(roleId);
                allPolicies.AddRange(rolePolicies);
            }

            // Filter active policies
            return allPolicies.Where(p => p.IsActive).DistinctBy(p => p.Id);
        }

        public async Task<AttributeValueDto?> GetAttributeValueAsync(long userId, string attributeName)
        {
            return await _attributeService.GetAttributeValueAsync(userId, attributeName);
        }

        public async Task<IEnumerable<AttributeValueDto>> GetUserAttributesAsync(long userId)
        {
            return await _attributeService.GetUserAttributesAsync(userId);
        }

        public async Task<bool> ValidatePolicyRuleAsync(PolicyRuleDto policyRule, object? resource, object? context)
        {
            var conditions = policyRule.PolicyConditions.OrderBy(c => c.Order).ToList();
            if (!conditions.Any()) return true;

            var result = true;

            foreach (var condition in conditions)
            {
                // We need userId for user attributes, but this method doesn't have it
                // For now, we'll use a default approach
                var conditionResult = await EvaluateConditionAsync(condition, resource, context, 0);
                
                if (condition.LogicalOperator == "AND")
                    result = result && conditionResult;
                else if (condition.LogicalOperator == "OR")
                    result = result || conditionResult;
            }

            return result;
        }

        private async Task<bool> EvaluateConditionAsync(PolicyConditionDto condition, object? resource, object? context, long userId)
        {
            try
            {
                // Get attribute value based on source
                var attributeValue = await GetAttributeValueForConditionAsync(condition, resource, context, userId);
                if (attributeValue == null) return false;

                // Get operator
                var operators = await _conditionOperatorService.GetActiveOperatorsAsync();
                var conditionOperator = operators.FirstOrDefault(o => o.Id == condition.ConditionOperatorId);
                if (conditionOperator == null) return false;

                // Apply operator
                return ApplyOperator(attributeValue.Value, conditionOperator, condition.Value);
            }
            catch (Exception)
            {
                return false; // Fail safe
            }
        }

        private async Task<PolicyEvaluationResultDto> EvaluatePolicyDetailedAsync(PolicyRuleDto policyRule, object? resource, object? context, long userId)
        {
            var policyResult = new PolicyEvaluationResultDto
            {
                PolicyRuleId = policyRule.Id,
                PolicyName = policyRule.Name,
                Effect = policyRule.Effect,
                Priority = policyRule.Priority,
                IsApplicable = true
            };

            var conditions = policyRule.PolicyConditions.OrderBy(c => c.Order).ToList();
            if (!conditions.Any())
            {
                policyResult.ConditionsMet = true;
                policyResult.Reason = "هیچ شرطی تعریف نشده، قانون همیشه اعمال می‌شود";
                return policyResult;
            }

            var allConditionsMet = true;
            var conditionResults = new List<ConditionEvaluationResultDto>();

            foreach (var condition in conditions)
            {
                var conditionResult = await EvaluateConditionDetailedAsync(condition, resource, context, userId);
                conditionResults.Add(conditionResult);

                if (condition.LogicalOperator == "AND")
                    allConditionsMet = allConditionsMet && conditionResult.IsMet;
                else if (condition.LogicalOperator == "OR")
                    allConditionsMet = allConditionsMet || conditionResult.IsMet;
            }

            policyResult.ConditionResults = conditionResults;
            policyResult.ConditionsMet = allConditionsMet;
            policyResult.Reason = allConditionsMet 
                ? "تمام شرایط برآورده شده‌اند" 
                : "برخی شرایط برآورده نشده‌اند";

            return policyResult;
        }

        private async Task<ConditionEvaluationResultDto> EvaluateConditionDetailedAsync(PolicyConditionDto condition, object? resource, object? context, long userId)
        {
            var conditionResult = new ConditionEvaluationResultDto
            {
                ConditionId = condition.Id,
                AttributeName = condition.AttributeName,
                Operator = condition.OperatorName,
                ExpectedValue = condition.Value
            };

            try
            {
                // Get attribute value based on source
                var attributeValue = await GetAttributeValueForConditionAsync(condition, resource, context, userId);
                if (attributeValue == null)
                {
                    conditionResult.IsMet = false;
                    conditionResult.ActualValue = "null";
                    conditionResult.Reason = "مقدار ویژگی یافت نشد";
                    return conditionResult;
                }

                conditionResult.ActualValue = attributeValue.Value;

                // Get operator
                var operators = await _conditionOperatorService.GetActiveOperatorsAsync();
                var conditionOperator = operators.FirstOrDefault(o => o.Id == condition.ConditionOperatorId);
                if (conditionOperator == null)
                {
                    conditionResult.IsMet = false;
                    conditionResult.Reason = "عملگر شرط یافت نشد";
                    return conditionResult;
                }

                // Apply operator
                conditionResult.IsMet = ApplyOperator(attributeValue.Value, conditionOperator, condition.Value);
                conditionResult.Reason = conditionResult.IsMet 
                    ? "شرط برآورده شد" 
                    : "شرط برآورده نشد";

                return conditionResult;
            }
            catch (Exception ex)
            {
                conditionResult.IsMet = false;
                conditionResult.ActualValue = "error";
                conditionResult.Reason = $"خطا در ارزیابی شرط: {ex.Message}";
                return conditionResult;
            }
        }

        private async Task<AttributeValueDto?> GetAttributeValueForConditionAsync(PolicyConditionDto condition, object? resource, object? context, long userId)
        {
            // Get attribute definition
            var attributeDef = await _attributeService.GetAttributeDefinitionByNameAsync(condition.AttributeName);
            if (attributeDef == null) return null;

            // Get attribute value based on source
            switch (attributeDef.Source.ToLower())
            {
                case "user":
                    return await _attributeService.GetAttributeValueAsync(userId, condition.AttributeName);
                
                case "resource":
                    if (resource != null)
                    {
                        // Extract resource ID from resource object
                        var resourceId = ExtractResourceId(resource);
                        if (resourceId.HasValue)
                        {
                            var resourceAttributes = await _attributeService.GetResourceAttributesAsync(resourceId.Value, resource.GetType().Name);
                            return resourceAttributes.FirstOrDefault(av => av.AttributeName == condition.AttributeName);
                        }
                    }
                    break;
                
                case "environment":
                    return await GetEnvironmentAttributeAsync(condition.AttributeName, context);
                
                case "action":
                    return await GetActionAttributeAsync(condition.AttributeName, context);
            }

            return null;
        }

        private long? ExtractResourceId(object resource)
        {
            // Try to get Id property from resource object
            var idProperty = resource.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var value = idProperty.GetValue(resource);
                if (value is long longValue) return longValue;
                if (value is int intValue) return intValue;
            }
            return null;
        }

        private async Task<AttributeValueDto?> GetEnvironmentAttributeAsync(string attributeName, object? context)
        {
            // Handle environment attributes like current time, IP address, etc.
            switch (attributeName.ToLower())
            {
                case "currenttime":
                    return new AttributeValueDto
                    {
                        Value = DateTime.UtcNow.ToString("HH:mm"),
                        IsActive = true
                    };
                
                case "currentdate":
                    return new AttributeValueDto
                    {
                        Value = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        IsActive = true
                    };
                
                default:
                    return null;
            }
        }

        private async Task<AttributeValueDto?> GetActionAttributeAsync(string attributeName, object? context)
        {
            // Handle action-specific attributes
            // This would typically come from the context object
            return null;
        }

        private bool ApplyOperator(string actualValue, ConditionOperatorDto conditionOperator, string expectedValue)
        {
            try
            {
                switch (conditionOperator.Name.ToLower())
                {
                    case "equals":
                        return actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
                    
                    case "contains":
                        return actualValue.Contains(expectedValue, StringComparison.OrdinalIgnoreCase);
                    
                    case "startswith":
                        return actualValue.StartsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
                    
                    case "endswith":
                        return actualValue.EndsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
                    
                    case "greaterthan":
                        if (decimal.TryParse(actualValue, out var actualNum) && decimal.TryParse(expectedValue, out var expectedNum))
                            return actualNum > expectedNum;
                        return false;
                    
                    case "lessthan":
                        if (decimal.TryParse(actualValue, out var actualNum2) && decimal.TryParse(expectedValue, out var expectedNum2))
                            return actualNum2 < expectedNum2;
                        return false;
                    
                    case "between":
                        var parts = expectedValue.Split('-');
                        if (parts.Length == 2 && decimal.TryParse(actualValue, out var actualNum3) &&
                            decimal.TryParse(parts[0], out var min) && decimal.TryParse(parts[1], out var max))
                            return actualNum3 >= min && actualNum3 <= max;
                        return false;
                    
                    case "in":
                        var values = expectedValue.Split(',');
                        return values.Any(v => actualValue.Equals(v.Trim(), StringComparison.OrdinalIgnoreCase));
                    
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false; // Fail safe
            }
        }
    }
} 
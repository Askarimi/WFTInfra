using AutoMapper;
using System.Linq.Expressions;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public class PolicyRuleService : IPolicyRuleService
    {
        private readonly IRepository<PolicyRule> _policyRuleRepository;
        private readonly IRepository<RolePolicyRule> _rolePolicyRuleRepository;
        private readonly IRepository<PolicyCondition> _policyConditionRepository;
        private readonly IMapper _mapper;

        public PolicyRuleService(
            IRepository<PolicyRule> policyRuleRepository,
            IRepository<RolePolicyRule> rolePolicyRuleRepository,
            IRepository<PolicyCondition> policyConditionRepository,
            IMapper mapper)
        {
            _policyRuleRepository = policyRuleRepository;
            _rolePolicyRuleRepository = rolePolicyRuleRepository;
            _policyConditionRepository = policyConditionRepository;
            _mapper = mapper;
        }

        public async Task<PolicyRuleDto> GetByIdAsync(long id)
        {
            var policyRule = await _policyRuleRepository.GetByIdAsync(id);
            return _mapper.Map<PolicyRuleDto>(policyRule);
        }

        public async Task<PolicyRuleDto> GetByNameAsync(string name)
        {
            var policyRule = await _policyRuleRepository.GetByExpressionAsync(r => r.Name == name);
            return _mapper.Map<PolicyRuleDto>(policyRule);
        }

        public async Task<IEnumerable<PolicyRuleDto>> GetAllAsync()
        {
            var policyRules = await _policyRuleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PolicyRuleDto>>(policyRules);
        }

        public async Task<IPagedList<PolicyRuleDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            Expression<Func<PolicyRule, bool>> filter = policy =>
                string.IsNullOrEmpty(request.SearchTerm) ||
                policy.Name.Contains(request.SearchTerm) ||
                policy.DisplayName.Contains(request.SearchTerm);

            if (request.Filters != null && request.Filters.Count > 0)
            {
                foreach (var filterItem in request.Filters)
                {
                    var property = typeof(PolicyRule).GetProperty(filterItem.Key);
                    if (property != null)
                    {
                        var param = Expression.Parameter(typeof(PolicyRule), "policy");
                        var left = Expression.Property(param, property);
                        var right = Expression.Constant(filterItem.Value);
                        var equalExpression = Expression.Equal(left, right);

                        filter = Expression.Lambda<Func<PolicyRule, bool>>(
                            Expression.AndAlso(filter.Body, equalExpression), param);
                    }
                }
            }

            var result = await _policyRuleRepository.GetPagedAsync(filter, request.PageNumber, request.PageSize);
            var policyRuleDtos = _mapper.Map<IEnumerable<PolicyRuleDto>>(result.Items);

            return new PagedList<PolicyRuleDto>(policyRuleDtos, result.TotalCount, result.PageNumber, result.PageSize);
        }

        public async Task<PolicyRuleDto> AddAsync(PolicyRuleDto dto)
        {
            var policyRule = _mapper.Map<PolicyRule>(dto);
            await _policyRuleRepository.AddAsync(policyRule);
            return _mapper.Map<PolicyRuleDto>(policyRule);
        }

        public async Task<PolicyRuleDto> UpdateAsync(PolicyRuleDto dto)
        {
            var policyRule = _mapper.Map<PolicyRule>(dto);
            await _policyRuleRepository.UpdateAsync(policyRule);
            return _mapper.Map<PolicyRuleDto>(policyRule);
        }

        public async Task DeleteAsync(long id)
        {
            await _policyRuleRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<PolicyRuleDto>> GetPoliciesForRoleAsync(long roleId)
        {
            var rolePolicyRules = await _rolePolicyRuleRepository
                .GetListByExpressionAsync(rpr => rpr.RoleId == roleId && rpr.IsActive);

            var policyRuleIds = rolePolicyRules.Select(rpr => rpr.PolicyRuleId).ToList();
            
            if (!policyRuleIds.Any())
                return Enumerable.Empty<PolicyRuleDto>();

            var policyRules = await _policyRuleRepository
                .GetListByExpressionAsync(pr => policyRuleIds.Contains(pr.Id) && pr.IsActive);

            return _mapper.Map<IEnumerable<PolicyRuleDto>>(policyRules);
        }

        public async Task<IEnumerable<PolicyRuleDto>> GetPoliciesForPermissionAsync(string permission)
        {
            // This would need to be implemented based on how permissions are linked to policies
            // For now, return empty collection
            return Enumerable.Empty<PolicyRuleDto>();
        }

        public async Task<bool> AddPolicyToRoleAsync(long roleId, long policyRuleId)
        {
            var existingRolePolicyRule = await _rolePolicyRuleRepository
                .GetByExpressionAsync(rpr => rpr.RoleId == roleId && rpr.PolicyRuleId == policyRuleId);

            if (existingRolePolicyRule != null)
                return false; // Already exists

            var rolePolicyRule = new RolePolicyRule
            {
                RoleId = roleId,
                PolicyRuleId = policyRuleId,
                IsActive = true
            };

            await _rolePolicyRuleRepository.AddAsync(rolePolicyRule);
            return true;
        }

        public async Task<bool> RemovePolicyFromRoleAsync(long roleId, long policyRuleId)
        {
            var rolePolicyRule = await _rolePolicyRuleRepository
                .GetByExpressionAsync(rpr => rpr.RoleId == roleId && rpr.PolicyRuleId == policyRuleId);

            if (rolePolicyRule == null)
                return false;

            await _rolePolicyRuleRepository.DeleteAsync(rolePolicyRule.Id);
            return true;
        }

        public async Task<IEnumerable<PolicyRuleDto>> GetActivePoliciesAsync()
        {
            var policyRules = await _policyRuleRepository
                .GetListByExpressionAsync(pr => pr.IsActive);

            return _mapper.Map<IEnumerable<PolicyRuleDto>>(policyRules);
        }
    }
} 
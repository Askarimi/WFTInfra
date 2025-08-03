using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IConditionOperatorService : IServiceBase<ConditionOperatorDto>
    {
        Task<IEnumerable<ConditionOperatorDto>> GetActiveOperatorsAsync();
        Task<IEnumerable<ConditionOperatorDto>> GetOperatorsByDataTypeAsync(string dataType);
    }
} 
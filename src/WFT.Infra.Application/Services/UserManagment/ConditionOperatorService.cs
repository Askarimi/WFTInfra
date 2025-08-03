using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public class ConditionOperatorService : IConditionOperatorService
    {
        private readonly IRepository<ConditionOperator> _conditionOperatorRepository;
        private readonly IMapper _mapper;

        public ConditionOperatorService(
            IRepository<ConditionOperator> conditionOperatorRepository,
            IMapper mapper)
        {
            _conditionOperatorRepository = conditionOperatorRepository;
            _mapper = mapper;
        }

        public async Task<ConditionOperatorDto> GetByIdAsync(long id)
        {
            var conditionOperator = await _conditionOperatorRepository.GetByIdAsync(id);
            return _mapper.Map<ConditionOperatorDto>(conditionOperator);
        }

        public async Task<ConditionOperatorDto> GetByNameAsync(string name)
        {
            var conditionOperator = await _conditionOperatorRepository.GetByExpressionAsync(co => co.Name == name);
            return _mapper.Map<ConditionOperatorDto>(conditionOperator);
        }

        public async Task<IEnumerable<ConditionOperatorDto>> GetAllAsync()
        {
            var conditionOperators = await _conditionOperatorRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ConditionOperatorDto>>(conditionOperators);
        }

        public async Task<IPagedList<ConditionOperatorDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            var conditionOperators = await _conditionOperatorRepository.GetAllAsync();
            var conditionOperatorDtos = _mapper.Map<IEnumerable<ConditionOperatorDto>>(conditionOperators);

            return new PagedList<ConditionOperatorDto>(conditionOperatorDtos, conditionOperatorDtos.Count(), 1, conditionOperatorDtos.Count());
        }

        public async Task<ConditionOperatorDto> AddAsync(ConditionOperatorDto dto)
        {
            var conditionOperator = _mapper.Map<ConditionOperator>(dto);
            await _conditionOperatorRepository.AddAsync(conditionOperator);
            return _mapper.Map<ConditionOperatorDto>(conditionOperator);
        }

        public async Task<ConditionOperatorDto> UpdateAsync(ConditionOperatorDto dto)
        {
            var conditionOperator = _mapper.Map<ConditionOperator>(dto);
            await _conditionOperatorRepository.UpdateAsync(conditionOperator);
            return _mapper.Map<ConditionOperatorDto>(conditionOperator);
        }

        public async Task DeleteAsync(long id)
        {
            await _conditionOperatorRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ConditionOperatorDto>> GetActiveOperatorsAsync()
        {
            var conditionOperators = await _conditionOperatorRepository
                .GetListByExpressionAsync(co => co.IsActive);

            return _mapper.Map<IEnumerable<ConditionOperatorDto>>(conditionOperators);
        }

        public async Task<IEnumerable<ConditionOperatorDto>> GetOperatorsByDataTypeAsync(string dataType)
        {
            var conditionOperators = await _conditionOperatorRepository
                .GetListByExpressionAsync(co => co.IsActive && co.DataTypes.Contains(dataType));

            return _mapper.Map<IEnumerable<ConditionOperatorDto>>(conditionOperators);
        }
    }
} 
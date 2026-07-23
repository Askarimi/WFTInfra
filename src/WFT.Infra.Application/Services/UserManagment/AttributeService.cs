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
    public class AttributeService : IAttributeService
    {
        private readonly IRepository<AttributeDefinition> _attributeDefinitionRepository;
        private readonly IRepository<AttributeValue> _attributeValueRepository;
        private readonly IMapper _mapper;

        public AttributeService(
            IRepository<AttributeDefinition> attributeDefinitionRepository,
            IRepository<AttributeValue> attributeValueRepository,
            IMapper mapper)
        {
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _attributeValueRepository = attributeValueRepository;
            _mapper = mapper;
        }

        public async Task<AttributeDefinitionDto> GetByIdAsync(long id)
        {
            var attributeDefinition = await _attributeDefinitionRepository.GetByIdAsync(id);
            return _mapper.Map<AttributeDefinitionDto>(attributeDefinition);
        }

        public async Task<AttributeDefinitionDto> GetByNameAsync(string name)
        {
            var attributeDefinition = await _attributeDefinitionRepository.GetByExpressionAsync(ad => ad.Name == name);
            return _mapper.Map<AttributeDefinitionDto>(attributeDefinition);
        }

        public async Task<IEnumerable<AttributeDefinitionDto>> GetAllAsync()
        {
            var attributeDefinitions = await _attributeDefinitionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AttributeDefinitionDto>>(attributeDefinitions);
        }

        public async Task<IPagedList<AttributeDefinitionDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            Expression<Func<AttributeDefinition, bool>> filter = attribute =>
                string.IsNullOrEmpty(request.SearchTerm) ||
                attribute.Name.Contains(request.SearchTerm) ||
                attribute.DisplayName.Contains(request.SearchTerm);

            if (request.Filters != null && request.Filters.Count > 0)
            {
                foreach (var filterItem in request.Filters)
                {
                    var property = typeof(AttributeDefinition).GetProperty(filterItem.Key);
                    if (property != null)
                    {
                        var param = Expression.Parameter(typeof(AttributeDefinition), "attribute");
                        var left = Expression.Property(param, property);
                        var right = Expression.Constant(filterItem.Value);
                        var equalExpression = Expression.Equal(left, right);

                        filter = Expression.Lambda<Func<AttributeDefinition, bool>>(
                            Expression.AndAlso(filter.Body, equalExpression), param);
                    }
                }
            }

            var result = await _attributeDefinitionRepository.GetPagedAsync(filter, request.PageNumber, request.PageSize);
            var attributeDefinitionDtos = _mapper.Map<IEnumerable<AttributeDefinitionDto>>(result.Items);

            return new PagedList<AttributeDefinitionDto>(attributeDefinitionDtos, result.TotalCount, result.PageNumber, result.PageSize);
        }

        public async Task<AttributeDefinitionDto> AddAsync(AttributeDefinitionDto dto)
        {
            var attributeDefinition = _mapper.Map<AttributeDefinition>(dto);
            await _attributeDefinitionRepository.AddAsync(attributeDefinition);
            return _mapper.Map<AttributeDefinitionDto>(attributeDefinition);
        }

        public async Task<AttributeDefinitionDto> UpdateAsync(AttributeDefinitionDto dto)
        {
            var attributeDefinition = _mapper.Map<AttributeDefinition>(dto);
            await _attributeDefinitionRepository.UpdateAsync(attributeDefinition);
            return _mapper.Map<AttributeDefinitionDto>(attributeDefinition);
        }

        public async Task DeleteAsync(long id)
        {
            await _attributeDefinitionRepository.DeleteAsync(id);
        }

        public async Task<AttributeValueDto?> GetAttributeValueAsync(long userId, string attributeName)
        {
            // Get attribute definition first
            var attributeDefinition = await _attributeDefinitionRepository
                .GetByExpressionAsync(ad => ad.Name == attributeName && ad.IsActive);

            if (attributeDefinition == null)
                return null;

            // Get attribute value for the user
            var attributeValue = await _attributeValueRepository
                .GetByExpressionAsync(av => 
                    av.AttributeDefinitionId == attributeDefinition.Id && 
                    av.UserId == userId && 
                    av.IsActive &&
                    (!av.ValidFrom.HasValue || av.ValidFrom <= DateTime.UtcNow) &&
                    (!av.ValidTo.HasValue || av.ValidTo >= DateTime.UtcNow));

            if (attributeValue == null)
                return null;

            var dto = _mapper.Map<AttributeValueDto>(attributeValue);
            dto.AttributeName = attributeDefinition.Name;
            return dto;
        }

        public async Task<bool> SetAttributeValueAsync(AttributeValueDto attributeValue)
        {
            try
            {
                var entity = _mapper.Map<AttributeValue>(attributeValue);
                
                // Check if value already exists
                var existingValue = await _attributeValueRepository
                    .GetByExpressionAsync(av => 
                        av.AttributeDefinitionId == entity.AttributeDefinitionId &&
                        av.UserId == entity.UserId &&
                        av.ResourceId == entity.ResourceId &&
                        av.ResourceType == entity.ResourceType);

                if (existingValue != null)
                {
                    // Update existing value
                    existingValue.Value = entity.Value;
                    existingValue.ValidFrom = entity.ValidFrom;
                    existingValue.ValidTo = entity.ValidTo;
                    existingValue.IsActive = entity.IsActive;
                    await _attributeValueRepository.UpdateAsync(existingValue);
                }
                else
                {
                    // Add new value
                    await _attributeValueRepository.AddAsync(entity);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<AttributeValueDto>> GetUserAttributesAsync(long userId)
        {
            var attributeValues = await _attributeValueRepository
                .GetListByExpressionAsync(av => 
                    av.UserId == userId && 
                    av.IsActive &&
                    (!av.ValidFrom.HasValue || av.ValidFrom <= DateTime.UtcNow) &&
                    (!av.ValidTo.HasValue || av.ValidTo >= DateTime.UtcNow));

            var dtos = new List<AttributeValueDto>();
            foreach (var av in attributeValues)
            {
                var dto = _mapper.Map<AttributeValueDto>(av);
                // Get attribute name
                var attributeDefinition = await _attributeDefinitionRepository.GetByIdAsync(av.AttributeDefinitionId);
                dto.AttributeName = attributeDefinition?.Name ?? string.Empty;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<IEnumerable<AttributeValueDto>> GetResourceAttributesAsync(long resourceId, string resourceType)
        {
            var attributeValues = await _attributeValueRepository
                .GetListByExpressionAsync(av => 
                    av.ResourceId == resourceId && 
                    av.ResourceType == resourceType &&
                    av.IsActive &&
                    (!av.ValidFrom.HasValue || av.ValidFrom <= DateTime.UtcNow) &&
                    (!av.ValidTo.HasValue || av.ValidTo >= DateTime.UtcNow));

            var dtos = new List<AttributeValueDto>();
            foreach (var av in attributeValues)
            {
                var dto = _mapper.Map<AttributeValueDto>(av);
                // Get attribute name
                var attributeDefinition = await _attributeDefinitionRepository.GetByIdAsync(av.AttributeDefinitionId);
                dto.AttributeName = attributeDefinition?.Name ?? string.Empty;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<IEnumerable<AttributeDefinitionDto>> GetAttributeDefinitionsBySourceAsync(string source)
        {
            var attributeDefinitions = await _attributeDefinitionRepository
                .GetListByExpressionAsync(ad => ad.Source == source && ad.IsActive);

            return _mapper.Map<IEnumerable<AttributeDefinitionDto>>(attributeDefinitions);
        }

        public async Task<AttributeDefinitionDto?> GetAttributeDefinitionByNameAsync(string name)
        {
            var attributeDefinition = await _attributeDefinitionRepository
                .GetByExpressionAsync(ad => ad.Name == name && ad.IsActive);

            return _mapper.Map<AttributeDefinitionDto>(attributeDefinition);
        }
    }
} 
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
    public class AttributeGroupService : IAttributeGroupService
    {
        private readonly IRepository<AttributeGroup> _attributeGroupRepository;
        private readonly IRepository<AttributeDefinition> _attributeDefinitionRepository;
        private readonly IMapper _mapper;

        public AttributeGroupService(
            IRepository<AttributeGroup> attributeGroupRepository,
            IRepository<AttributeDefinition> attributeDefinitionRepository,
            IMapper mapper)
        {
            _attributeGroupRepository = attributeGroupRepository;
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _mapper = mapper;
        }

        public async Task<AttributeGroupDto> GetByIdAsync(long id)
        {
            var attributeGroup = await _attributeGroupRepository.GetByIdAsync(id);
            var dto = _mapper.Map<AttributeGroupDto>(attributeGroup);
            
            if (dto != null)
            {
                dto.AttributeCount = await GetAttributeCountForGroupAsync(id);
            }
            
            return dto;
        }

        public async Task<AttributeGroupDto> GetByNameAsync(string name)
        {
            var attributeGroup = await _attributeGroupRepository.GetByExpressionAsync(ag => ag.Name == name);
            var dto = _mapper.Map<AttributeGroupDto>(attributeGroup);
            
            if (dto != null)
            {
                dto.AttributeCount = await GetAttributeCountForGroupAsync(dto.Id);
            }
            
            return dto;
        }

        public async Task<IEnumerable<AttributeGroupDto>> GetAllAsync()
        {
            var attributeGroups = await _attributeGroupRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<AttributeGroupDto>>(attributeGroups);
            
            // Add attribute count for each group
            foreach (var dto in dtos)
            {
                dto.AttributeCount = await GetAttributeCountForGroupAsync(dto.Id);
            }
            
            return dtos;
        }

        public async Task<IPagedList<AttributeGroupDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            Expression<Func<AttributeGroup, bool>> filter = group =>
                string.IsNullOrEmpty(request.SearchTerm) ||
                group.Name.Contains(request.SearchTerm) ||
                group.DisplayName.Contains(request.SearchTerm);

            if (request.Filters != null && request.Filters.Count > 0)
            {
                foreach (var filterItem in request.Filters)
                {
                    var property = typeof(AttributeGroup).GetProperty(filterItem.Key);
                    if (property != null)
                    {
                        var param = Expression.Parameter(typeof(AttributeGroup), "group");
                        var left = Expression.Property(param, property);
                        var right = Expression.Constant(filterItem.Value);
                        var equalExpression = Expression.Equal(left, right);

                        filter = Expression.Lambda<Func<AttributeGroup, bool>>(
                            Expression.AndAlso(filter.Body, equalExpression), param);
                    }
                }
            }

            var result = await _attributeGroupRepository.GetPagedAsync(filter, request.PageNumber, request.PageSize);
            var attributeGroupDtos = _mapper.Map<IEnumerable<AttributeGroupDto>>(result.Items);

            // Add attribute count for each group
            foreach (var dto in attributeGroupDtos)
            {
                dto.AttributeCount = await GetAttributeCountForGroupAsync(dto.Id);
            }

            return new PagedList<AttributeGroupDto>(attributeGroupDtos, result.TotalCount, result.PageNumber, result.PageSize);
        }

        public async Task<AttributeGroupDto> AddAsync(AttributeGroupDto dto)
        {
            // Check if name already exists
            var existingGroup = await _attributeGroupRepository.GetByExpressionAsync(ag => ag.Name == dto.Name);
            if (existingGroup != null)
                throw new InvalidOperationException("گروه ویژگی با این نام قبلاً وجود دارد");

            var attributeGroup = _mapper.Map<AttributeGroup>(dto);
            await _attributeGroupRepository.AddAsync(attributeGroup);
            return _mapper.Map<AttributeGroupDto>(attributeGroup);
        }

        public async Task<AttributeGroupDto> UpdateAsync(AttributeGroupDto dto)
        {
            var attributeGroup = _mapper.Map<AttributeGroup>(dto);
            await _attributeGroupRepository.UpdateAsync(attributeGroup);
            return _mapper.Map<AttributeGroupDto>(attributeGroup);
        }

        public async Task DeleteAsync(long id)
        {
            // Check if group has attributes
            var attributeCount = await GetAttributeCountForGroupAsync(id);
            if (attributeCount > 0)
                throw new InvalidOperationException("نمی‌توان گروهی که دارای ویژگی است را حذف کرد");

            await _attributeGroupRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<AttributeGroupDto>> GetActiveGroupsAsync()
        {
            var attributeGroups = await _attributeGroupRepository.GetListByExpressionAsync(ag => ag.IsActive);
            var dtos = _mapper.Map<IEnumerable<AttributeGroupDto>>(attributeGroups);
            
            // Add attribute count for each group
            foreach (var dto in dtos)
            {
                dto.AttributeCount = await GetAttributeCountForGroupAsync(dto.Id);
            }
            
            return dtos;
        }

        public async Task<IEnumerable<AttributeGroupDto>> GetGroupsBySortOrderAsync()
        {
            var attributeGroups = await _attributeGroupRepository.GetAllAsync();
            var sortedGroups = attributeGroups.OrderBy(ag => ag.SortOrder).ThenBy(ag => ag.DisplayName);
            var dtos = _mapper.Map<IEnumerable<AttributeGroupDto>>(sortedGroups);
            
            // Add attribute count for each group
            foreach (var dto in dtos)
            {
                dto.AttributeCount = await GetAttributeCountForGroupAsync(dto.Id);
            }
            
            return dtos;
        }

        public async Task<bool> AssignAttributeToGroupAsync(long attributeId, long groupId)
        {
            // Verify group exists
            var group = await _attributeGroupRepository.GetByIdAsync(groupId);
            if (group == null) return false;

            // Update attribute definition
            var attribute = await _attributeDefinitionRepository.GetByIdAsync(attributeId);
            if (attribute == null) return false;

            attribute.AttributeGroupId = groupId;
            await _attributeDefinitionRepository.UpdateAsync(attribute);

            return true;
        }

        public async Task<bool> RemoveAttributeFromGroupAsync(long attributeId)
        {
            var attribute = await _attributeDefinitionRepository.GetByIdAsync(attributeId);
            if (attribute == null) return false;

            attribute.AttributeGroupId = null;
            await _attributeDefinitionRepository.UpdateAsync(attribute);

            return true;
        }

        public async Task<bool> UpdateSortOrderAsync(long groupId, int newSortOrder)
        {
            var group = await _attributeGroupRepository.GetByIdAsync(groupId);
            if (group == null) return false;

            group.SortOrder = newSortOrder;
            await _attributeGroupRepository.UpdateAsync(group);

            return true;
        }

        private async Task<int> GetAttributeCountForGroupAsync(long groupId)
        {
            var attributes = await _attributeDefinitionRepository.GetListByExpressionAsync(ad => ad.AttributeGroupId == groupId && ad.IsActive);
            return attributes?.Count() ?? 0;
        }
    }
} 
using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IAttributeGroupService : IServiceBase<AttributeGroupDto>
    {
        Task<IEnumerable<AttributeGroupDto>> GetActiveGroupsAsync();
        Task<IEnumerable<AttributeGroupDto>> GetGroupsBySortOrderAsync();
        Task<bool> AssignAttributeToGroupAsync(long attributeId, long groupId);
        Task<bool> RemoveAttributeFromGroupAsync(long attributeId);
        Task<bool> UpdateSortOrderAsync(long groupId, int newSortOrder);
    }
} 
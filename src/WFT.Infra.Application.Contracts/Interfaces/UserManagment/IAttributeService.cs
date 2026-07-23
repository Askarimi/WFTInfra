using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IAttributeService : IServiceBase<AttributeDefinitionDto>
    {
        Task<AttributeValueDto?> GetAttributeValueAsync(long userId, string attributeName);
        Task<bool> SetAttributeValueAsync(AttributeValueDto attributeValue);
        Task<IEnumerable<AttributeValueDto>> GetUserAttributesAsync(long userId);
        Task<IEnumerable<AttributeValueDto>> GetResourceAttributesAsync(long resourceId, string resourceType);
        Task<IEnumerable<AttributeDefinitionDto>> GetAttributeDefinitionsBySourceAsync(string source);
        Task<AttributeDefinitionDto?> GetAttributeDefinitionByNameAsync(string name);
    }
} 
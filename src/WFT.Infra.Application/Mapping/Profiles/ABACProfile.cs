using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Mapping.Profiles
{
    public class ABACProfile : Profile
    {
        public ABACProfile()
        {
            // AttributeGroup mappings
            CreateMap<AttributeGroup, AttributeGroupDto>().ReverseMap();

            // AttributeDefinition mappings
            CreateMap<AttributeDefinition, AttributeDefinitionDto>()
     .ForMember(dest => dest.AttributeGroupName, opt => opt.MapFrom(src => src.AttributeGroup != null ? src.AttributeGroup.DisplayName : null))
     .ReverseMap()
     .ForMember(dest => dest.AttributeGroup, opt => opt.Ignore()); // مهم!

            // AttributeValue mappings
            CreateMap<AttributeValue, AttributeValueDto>().ReverseMap();

            // PolicyRule mappings
            CreateMap<PolicyRule, PolicyRuleDto>().ReverseMap();

            // PolicyCondition mappings
            CreateMap<PolicyCondition, PolicyConditionDto>().ReverseMap();

            // ConditionOperator mappings
            CreateMap<ConditionOperator, ConditionOperatorDto>().ReverseMap();

            // RolePolicyRule mappings
            CreateMap<RolePolicyRule, object>().ReverseMap(); // No DTO needed for this junction table
        }
    }
}
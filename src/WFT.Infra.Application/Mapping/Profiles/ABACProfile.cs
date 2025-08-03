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
            CreateMap<AttributeGroup, AttributeGroupDto>()
                .ForMember(dest => dest.AttributeCount, opt => opt.Ignore())
                .ForMember(dest => dest.AttributeDefinitions, opt => opt.MapFrom(src => src.AttributeDefinitions))
                .ReverseMap();

            // AttributeDefinition mappings
            CreateMap<AttributeDefinition, AttributeDefinitionDto>()
                .ForMember(dest => dest.AttributeGroupName, opt => opt.MapFrom(src => src.AttributeGroup != null ? src.AttributeGroup.DisplayName : null))
                .ReverseMap();

            // AttributeValue mappings
            CreateMap<AttributeValue, AttributeValueDto>()
                .ForMember(dest => dest.AttributeName, opt => opt.Ignore())
                .ReverseMap();

            // PolicyRule mappings
            CreateMap<PolicyRule, PolicyRuleDto>()
                .ForMember(dest => dest.PolicyConditions, opt => opt.MapFrom(src => src.PolicyConditions))
                .ReverseMap();

            // PolicyCondition mappings
            CreateMap<PolicyCondition, PolicyConditionDto>()
                .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => src.AttributeDefinition.Name))
                .ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.ConditionOperator.Name))
                .ReverseMap();

            // ConditionOperator mappings
            CreateMap<ConditionOperator, ConditionOperatorDto>().ReverseMap();

            // RolePolicyRule mappings
            CreateMap<RolePolicyRule, object>().ReverseMap(); // No DTO needed for this junction table
        }
    }
} 
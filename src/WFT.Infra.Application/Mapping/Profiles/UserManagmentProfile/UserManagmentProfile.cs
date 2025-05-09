using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Mapping.Profiles.UserManagmentProfile
{
    public class UserManagmentProfile : BaseProfile
    {
        protected override void ConfigureMappings()
        {

            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();

        }
    }
}

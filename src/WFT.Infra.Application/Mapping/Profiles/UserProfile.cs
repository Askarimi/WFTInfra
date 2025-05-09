using RPK.Infra.Application.Contracts.DTOs;
using RPK.Infra.Application.Contracts.Models.User;
using RPK.Infra.Core.Entities;

namespace RPK.Infra.Application.Mapping.Profiles
{
    public class UserProfile : BaseProfile
    {
        protected override void ConfigureMappings()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<User, UserModel>();
            CreateMap<UserModel, User>();
        }
    }
}

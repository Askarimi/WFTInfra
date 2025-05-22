using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Core.Entities;

namespace WFT.Infra.Application.Mapping.Profiles
{
    public partial class GlobalEntitesProfile : BaseProfile
    {
        protected override void ConfigureMappings()
        {
            CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();
        }
    }
}

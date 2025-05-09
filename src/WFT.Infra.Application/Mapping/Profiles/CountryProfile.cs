using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Core.Entities;

namespace WFT.Infra.Application.Mapping.Profiles
{
    public class CountryProfile : BaseProfile
    {
        protected override void ConfigureMappings()
        {
            CreateMap<Country, CountryDto>().ReverseMap();

        }
    }
}

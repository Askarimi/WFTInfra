using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities;

namespace WFT.Infra.Application.Services
{
    public partial class CountryService : ICountryService
    {
        #region ctor
        private readonly IMapper _mapper;
        private readonly IRepository<Country> _countryRepository;

        public CountryService(IRepository<Country> countryRepository, IMapper mapper)
        {
            _countryRepository = countryRepository;
            _mapper = mapper;
        }
        #endregion
        public virtual async Task<CountryDto> CreateAsync(CountryDto dto)
        {
            var country = _mapper.Map<Country>(dto);

            await _countryRepository.CreateAsync(country);

            var countryDto = _mapper.Map<CountryDto>(country);

            return countryDto;
        }

        public virtual async Task DeleteAsync(int id)
        {
            await _countryRepository.DeleteAsync(id);
        }

        public Task<IEnumerable<CountryDto>> GetActiveSamplesAsync()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<CountryDto>> GetAllAsync()
        {
            var countries = await _countryRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<CountryDto>>(countries);
        }

        public virtual async Task<CountryDto> GetByIdAsync(int id)
        {
            var country = await _countryRepository.GetByIdAsync(id);

            return _mapper.Map<CountryDto>(country);
        }

        public virtual async Task UpdateAsync(CountryDto dto)
        {
            var country = _mapper.Map<Country>(dto);

            await _countryRepository.UpdateAsync(country);
        }
    }
}

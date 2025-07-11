using AutoMapper;
using System.Linq.Expressions;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Models;
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
        public virtual async Task<CountryDto> AddAsync(CountryDto dto)
        {
            var country = _mapper.Map<Country>(dto);

            await _countryRepository.AddAsync(country);

            var countryDto = _mapper.Map<CountryDto>(country);

            return countryDto;
        }

        public virtual async Task DeleteAsync(long id)
        {
            await _countryRepository.DeleteAsync(id);
        }

        public virtual async Task<IEnumerable<CountryDto>> GetAllAsync()
        {
            var countries = await _countryRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<CountryDto>>(countries);
        }

        public virtual async Task<CountryDto> GetByIdAsync(long id)
        {
            var country = await _countryRepository.GetByIdAsync(id);

            return _mapper.Map<CountryDto>(country);
        }

        public virtual async Task<CountryDto> GetByNameAsync(string name)
        {
            var country = await _countryRepository.GetByExpressionAsync(c => c.Name.Contains(name));

            return _mapper.Map<CountryDto>(country);
        }

        public async Task<IPagedList<CountryDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            // شروع از یک فیلتر پایه برای جستجوی عمومی
            Expression<Func<Country, bool>> filter = country =>
                string.IsNullOrEmpty(request.SearchTerm) ||
                country.Name.Contains(request.SearchTerm);

            // اگر فیلترهای اضافی (Filters) وجود دارند، آن‌ها را اضافه می‌کنیم
            if (request.Filters != null && request.Filters.Count > 0)
            {
                foreach (var filterItem in request.Filters)
                {
                    var property = typeof(Country).GetProperty(filterItem.Key);
                    if (property != null)
                    {
                        var param = Expression.Parameter(typeof(Country), "country");
                        var left = Expression.Property(param, property);
                        var right = Expression.Constant(filterItem.Value);
                        var equalExpression = Expression.Equal(left, right);

                        // ترکیب فیلترهای قبلی با فیلتر جدید
                        filter = Expression.Lambda<Func<Country, bool>>(Expression.AndAlso(filter.Body, equalExpression), param);
                    }
                }
            }

            // دریافت داده‌ها با صفحه‌بندی
            var result = await _countryRepository.GetPagedAsync(filter, request.PageNumber, request.PageSize);

            // تبدیل به CountryDto با استفاده از AutoMapper
            var countryDtos = _mapper.Map<IEnumerable<CountryDto>>(result.Items);

            // بازگشت نتایج صفحه‌بندی‌شده
            return new PagedList<CountryDto>(countryDtos, result.TotalCount, result.PageNumber, result.PageSize);
        }

        public virtual async Task<CountryDto> UpdateAsync(CountryDto dto)
        {
            var country = _mapper.Map<Country>(dto);

            await _countryRepository.UpdateAsync(country);

            var countryDto = _mapper.Map<CountryDto>(country);

            return countryDto;

        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class CountryController : BaseController
    {
        private readonly ICountryService _countryService;

        // تزریق سرویس ICountryService به constructor کنترلر
        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        // متد GetAll برای دریافت لیست همه کشورهای موجود
        [HttpGet]
        [Route("list")]
        public async Task<IActionResult> GetAllCountries()
        {
            var countries = await _countryService.GetAllAsync();
            return SuccessResponse(countries);
        }

        // متد Create برای ایجاد یک کشور جدید
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> CreateCountry([FromBody] CountryDto countryDto)
        {
            if (countryDto == null || string.IsNullOrEmpty(countryDto.Name))
            {
                return ErrorResponse("Invalid country data", 400);
            }

            var createdCountry = await _countryService.AddAsync(countryDto);
            return CreatedResponse(createdCountry);
        }

        // متد GetById برای دریافت اطلاعات یک کشور با آی‌دی خاص
        [HttpGet]
        [Route("init/{id}")]
        public async Task<IActionResult> GetCountryById(int id)
        {
            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
            {
                return ErrorResponse("Country not found", 404);
            }
            return SuccessResponse(country);
        }

        // DELETE api/v1/country/{id}
        [HttpDelete()]
        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _countryService.DeleteAsync(id);

            return SuccessResponse("Country deleted successfully");
        }
        [HttpPut]
        [Route("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CountryDto dto)
        {
            if (dto == null)
            {
                return ErrorResponse("Invalid data", 400);
            }

            await _countryService.UpdateAsync(dto);

            return SuccessResponse("Country deleted successfully");
        }
    }
}

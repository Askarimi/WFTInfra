using WFT.Infra.Application.Contracts.DTOs;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface ISampleService
    {
        Task<SampleDto> GetByIdAsync(Guid id);
        Task<IEnumerable<SampleDto>> GetAllAsync();
        Task<IEnumerable<SampleDto>> GetActiveSamplesAsync();
        Task<SampleDto> CreateAsync(string name, string description);
        Task UpdateAsync(Guid id, string name, string description);
        Task DeleteAsync(Guid id);
    }
}
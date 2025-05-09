namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface IServiceBase<TDto> where TDto : class
    {
        Task<TDto> GetByIdAsync(int id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<IEnumerable<TDto>> GetActiveSamplesAsync();
        Task<TDto> CreateAsync(TDto dto);
        Task UpdateAsync(TDto dto);
        Task DeleteAsync(int id);
    }
}

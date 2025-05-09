namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface IServiceBase<TDto> where TDto : class
    {
        Task<TDto> GetByIdAsync(long id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<TDto> AddAsync(TDto dto);
        Task UpdateAsync(TDto dto);
        Task DeleteAsync(long id);
    }
}

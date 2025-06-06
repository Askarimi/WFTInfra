using WFT.Infra.Application.Contracts.Models;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface IServiceBase<TDto> where TDto : class
    {
        Task<TDto> GetByIdAsync(long id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<IPagedList<TDto>> GetPagedListAsync(PagedQueryRequest request);
        Task<TDto> GetByNameAsync(string name);
        Task<TDto> AddAsync(TDto dto);
        Task UpdateAsync(TDto dto);
        Task DeleteAsync(long id);
    }
}

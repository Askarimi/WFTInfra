using WFT.Infra.Application.Contracts.DTOs;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface IRefreshTokenService
    {
        Task AddAsync(RefreshTokenDto token);
        Task<RefreshTokenDto?> GetByTokenAsync(string token);
        Task<bool> IsValidAsync(string token);
        Task RevokeAsync(string token);
    }
}

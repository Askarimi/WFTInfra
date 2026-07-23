using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IUserPasswordService : IServiceBase<UserPasswordDto>
    {
        Task<UserPasswordDto> CreatePassword(UserPasswordDto dto);
        Task<UserPasswordDto> GetPasswordByUserIdAsync(long userId);
        
        // New methods for password management
        Task<bool> SetPasswordAsync(UserPasswordDto request);
        Task<bool> ChangePasswordAsync(UserPasswordDto request);
        
        // Method for authentication (returns password hash)
        Task<UserPasswordAuthDto> GetPasswordForAuthAsync(long userId);
    }
}

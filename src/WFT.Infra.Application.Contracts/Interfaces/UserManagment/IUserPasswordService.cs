using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IUserPasswordService
    {
        Task<UserPasswordDto> CreatePassword(UserPasswordDto dto);
        public Task<UserPasswordDto> GetPasswordByUserIdAsync(long userId);
    }
}

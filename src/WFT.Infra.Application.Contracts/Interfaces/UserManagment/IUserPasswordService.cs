using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public interface IUserPasswordService : IServiceBase<UserPasswordDto>
    {
        public Task<UserPasswordDto> GetPasswordByUserIdAsync(long userId);
    }
}

using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(UserRegisterDto userDto);
        Task<string> LoginAsync(string username, string password);
        Task<LoginResultDto> LoginAsync(UserLoginDto dto);
        Task LogoutAsync(string token);
    }
}
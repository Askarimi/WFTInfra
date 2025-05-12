using System.Security.Claims;
using System.Threading.Tasks;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(UserDto userDto, string password);
        Task<string> LoginAsync(string username, string password);
    }
} 
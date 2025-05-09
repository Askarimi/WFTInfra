using System.Security.Claims;
using System.Threading.Tasks;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface IAuthService
    {
        Task<string> GenerateToken(string username, string password);
        Task<ClaimsPrincipal> ValidateToken(string token);
    }
} 
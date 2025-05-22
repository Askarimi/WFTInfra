using System.Security.Claims;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface ITokenService
    {
        string GenerateToken(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);

        string GenerateTokenForUser(UserDto userDto);

        string RefreshToken(string expiredToken);
        Task<JwtResultDto?> RefreshTokenAsync(string refreshToken);

        bool ValidateToken(string token);

        public (string UserId, string Username, List<string> Roles, List<string> Permissions)? ExtractClaims(string token);

        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}

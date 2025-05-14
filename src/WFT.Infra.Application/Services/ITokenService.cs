using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services
{
    public partial interface ITokenService
    {
        string GenerateToken(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);

        string GenerateTokenForUser(User user);
        
        string RefreshToken(string expiredToken);

        bool ValidateToken(string token);

        public (string UserId, string Username, List<string> Roles, List<string> Permissions)? ExtractClaims(string token);

        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WFT.Infra.Application.Helper;
using WFT.Infra.Application.Settings;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services
{
    public partial class TokenService:ITokenService
    {
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtSettings _jwtSettings;
        public TokenService(JwtTokenGenerator jwtTokenGenerator,IOptions<JwtSettings> jwtOptions)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtSettings = jwtOptions.Value;    
        }

        public string GenerateToken(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            return _jwtTokenGenerator.GenerateToken(userId, username, roles, permissions);
        }

        public string GenerateTokenForUser(User user)
        {
            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name));

            return _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Username, roles, permissions);
        }
        public string RefreshToken(string expiredToken)
        {
            var principal = GetPrincipalFromToken(expiredToken);

            if (principal == null)
                throw new SecurityTokenException("Invalid token");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var username = principal.Identity?.Name
                           ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;

            var roles = principal.Claims
                                 .Where(c => c.Type == ClaimTypes.Role)
                                 .Select(c => c.Value);

            var permissions = principal.Claims
                                       .Where(c => c.Type == "permission")
                                       .Select(c => c.Value);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                throw new SecurityTokenException("Token is missing required claims");

            return _jwtTokenGenerator.GenerateToken(userId, username, roles, permissions);
        }

        public bool ValidateToken(string token)
        {
            var principal = GetPrincipalFromToken(token);
            return principal != null;
        }

        public (string UserId, string Username, List<string> Roles, List<string> Permissions)? ExtractClaims(string token)
        {
            var principal = GetPrincipalFromToken(token);
            if (principal == null)
                return null;

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var username = principal.Identity?.Name
                           ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;

            var roles = principal.Claims
                                 .Where(c => c.Type == ClaimTypes.Role)
                                 .Select(c => c.Value)
                                 .ToList();

            var permissions = principal.Claims
                                       .Where(c => c.Type == "permission")
                                       .Select(c => c.Value)
                                       .ToList();

            return (userId ?? "", username ?? "", roles, permissions);
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();



            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // اختیاری - پیش‌فرض 5 دقیقه است
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // اطمینان از اینکه توکن JWT است و الگوریتم درسته
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }


    }
}

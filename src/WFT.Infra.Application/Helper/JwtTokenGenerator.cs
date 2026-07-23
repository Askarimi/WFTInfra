using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using WFT.Infra.Application.Contracts.Settings;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Helper
{
    // این کلاس به زیرساخت منتقل شده است؛ فایل Application قابل حذف است در صورت عدم نیاز.
    public partial class JwtTokenGenerator:IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public JwtSettings JwtSettings { set { value = _jwtSettings; } }

        public string GenerateToken(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, username)
            };

            // اضافه کردن Roleها
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // اضافه کردن Permissionها
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

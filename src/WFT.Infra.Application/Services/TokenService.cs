using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Helper;
using WFT.Infra.Application.Settings;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services
{
    public partial class TokenService : ITokenService
    {
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        public TokenService(JwtTokenGenerator jwtTokenGenerator,
            IOptions<JwtSettings> jwtOptions,
            IMapper mapper,
            IRefreshTokenService refreshTokenService,
            IUserService userService,
            IRoleService roleService
            )
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtSettings = jwtOptions.Value;
            _mapper = mapper;
            _refreshTokenService = refreshTokenService;
            _userService = userService;
            _roleService = roleService;
        }

        public string GenerateToken(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            return _jwtTokenGenerator.GenerateToken(userId, username, roles, permissions);
        }

        public string GenerateTokenForUser(UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
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

        public async Task<JwtResultDto?> RefreshTokenAsync(string refreshToken)
        {
            var tokenRecord = await _refreshTokenService.GetByTokenAsync(refreshToken);

            if (tokenRecord == null || tokenRecord.IsRevoked || tokenRecord.ExpiresAt < DateTime.UtcNow)
                return null;

            var user = await _userService.GetByIdAsync(tokenRecord.UserId);
            if (user == null)
                return null;

            var roles = await _userService.GetRolesForUserAsync(user.Id);
            var permissions = await _roleService.GetPermissionsForRoleAsync(roles.Select(r => r.Id).ToList());

            // تولید AccessToken جدید
            var accessToken = _jwtTokenGenerator.GenerateToken(
                userId: user.Id.ToString(),
                username: user.Username,
                roles: roles.Select(r => r.Name),
                permissions: permissions.Select(p => p.Name)
            );

            // تولید RefreshToken جدید (اختیاری)
            var newRefreshToken = Guid.NewGuid().ToString("N");
            await _refreshTokenService.RevokeAsync(refreshToken); // توکن قبلی رو غیرفعال کن
            await _refreshTokenService.AddAsync(new RefreshTokenDto
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            return new JwtResultDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken // در کوکی گذاشته می‌شه
            };
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

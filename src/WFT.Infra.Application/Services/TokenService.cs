using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Application.Contracts.Settings;

namespace WFT.Infra.Application.Services
{
    public partial class TokenService : ITokenService
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        public TokenService(IJwtTokenGenerator jwtTokenGenerator,
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

        // New: GenerateToken via TokenRequest (contracts DTO)
        public string GenerateToken(TokenRequest request)
        {
            // If permissions not supplied explicitly via request.Claims, derive from roles if needed
            var roles = request.Roles ?? Enumerable.Empty<string>();
            var permissions = Enumerable.Empty<string>();
            return _jwtTokenGenerator.GenerateToken(request.UserId, request.UserName ?? string.Empty, roles, permissions);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            return GetPrincipalFromToken(token);
        }

        public IDictionary<string, string> ExtractClaims(string token)
        {
            var principal = GetPrincipalFromToken(token);
            if (principal == null) return new Dictionary<string, string>();

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var claim in principal.Claims)
            {
                if (!dict.ContainsKey(claim.Type))
                    dict[claim.Type] = claim.Value;
            }
            return dict;
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


        // Single ValidateToken retained in interface returns ClaimsPrincipal?

        // Removed tuple-based ExtractClaims; single IDictionary<string,string> version retained

        private ClaimsPrincipal? GetPrincipalFromToken(string token)
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

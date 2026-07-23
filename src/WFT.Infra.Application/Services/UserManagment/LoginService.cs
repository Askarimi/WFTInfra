using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    /// <summary>
    /// Enhanced login service that aggregates user roles and permissions
    /// </summary>
    public class LoginService : ILoginService
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<RolePermission> _rolePermissionRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IUserPasswordService _userPasswordService;
        private readonly IMapper _mapper;

        public LoginService(
            IUserService userService,
            IRoleService roleService,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService,
            IRepository<User> userRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<RolePermission> rolePermissionRepository,
            IRepository<Permission> permissionRepository,
            IUserPasswordService userPasswordService,
            IMapper mapper)
        {
            _userService = userService;
            _roleService = roleService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
            _userPasswordService = userPasswordService;
            _mapper = mapper;
        }

        /// <summary>
        /// Login user with enhanced response containing roles and permissions
        /// </summary>
        public async Task<LoginResponseDto?> LoginWithRolesAndPermissionsAsync(UserLoginDto dto)
        {
            try
            {
                // Get user
                var user = await _userRepository.GetByExpressionAsync(u => u.Username == dto.UserName);
                if (user == null)
                    return null;

                // Verify password
                var userPassword = await _userPasswordService.GetPasswordForAuthAsync(user.Id);
                if (userPassword == null || !BCrypt.Net.BCrypt.Verify(dto.Password, userPassword.PasswordHash))
                    return null;

                // Get user's roles
                var userRoles = await _userService.GetRolesForUserAsync(user.Id);
                var roleIds = userRoles.Select(r => r.Id).ToList();
                var roleNames = userRoles.Select(r => r.Name).ToList();

                // Get permissions for all roles
                var permissions = await _roleService.GetPermissionsForRoleAsync(roleIds);
                var permissionNames = permissions
                    .Select(p => p.Name)
                    .Distinct()
                    .ToList();

                // Generate tokens
                var tokenRequest = new TokenRequest(
                    user.Id.ToString(),
                    user.Username,
                    roleNames
                );
                var accessToken = _tokenService.GenerateToken(tokenRequest);
                var refreshToken = GenerateSecureToken();

                // Save refresh token
                var refreshTokenDto = new RefreshTokenDto
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                };
                await _refreshTokenService.AddAsync(refreshTokenDto);

                // Build response
                var loginUserDto = new LoginUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive
                };

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = loginUserDto,
                    Roles = roleNames,
                    Permissions = permissionNames
                };

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get aggregated permissions for a user from all assigned roles
        /// </summary>
        public async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId)
        {
            try
            {
                // Get user's roles
                var userRoles = await _userRoleRepository.GetListByExpressionAsync(ur => ur.UserId == userId);
                var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

                if (!roleIds.Any())
                    return Enumerable.Empty<string>();

                // Get all role permissions
                var rolePermissions = await _rolePermissionRepository.GetListByExpressionAsync(
                    rp => roleIds.Contains(rp.RoleId));

                var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();

                if (!permissionIds.Any())
                    return Enumerable.Empty<string>();

                // Get permission names
                var permissions = await _permissionRepository.GetListByExpressionAsync(
                    p => permissionIds.Contains(p.Id));

                return permissions.Select(p => p.Name).Distinct();
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Get user's assigned roles
        /// </summary>
        public async Task<IEnumerable<string>> GetUserRolesAsync(long userId)
        {
            try
            {
                var roles = await _userService.GetRolesForUserAsync(userId);
                return roles.Select(r => r.Name);
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Generate secure random token
        /// </summary>
        private string GenerateSecureToken()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var tokenData = new byte[64];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
        }
    }
}


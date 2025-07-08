using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private readonly IRepository<Role> _roleRepository;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserPasswordService _userPasswordService;
        public AuthService(IRepository<User> userRepository, IPasswordHasher passwordHasher,
                          IJwtTokenGenerator jwtTokenGenerator,
                          IMapper mapper,
                          IRepository<Role> roleRepository,
                          IUserService userService,
                          IRoleService roleService,
                          ITokenService tokenService,
                          IRefreshTokenService refreshTokenService,
                          IUserPasswordService userPasswordService
                          )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mapper = mapper;
            _roleRepository = roleRepository;
            _userService = userService;
            _roleService = roleService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _userPasswordService = userPasswordService;
        }


        public virtual async Task<LoginResultDto> LoginAsync(UserLoginDto dto)
        {
            var user = await _userRepository.GetByExpressionAsync(u => u.Username == dto.UserName);

            if (user == null)
                throw new Exception("User not found");

            var userPassword = await _userPasswordService.GetPasswordByUserIdAsync(user.Id);

            var userDto = _mapper.Map<UserDto>(user);


            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, userPassword.PasswordHash))
                throw new UnauthorizedAccessException("نام کاربری یا رمز عبور اشتباه است.");

            var roles = await _userService.GetRolesForUserAsync(user.Id); // فرض بر اینکه این متد وجود داره

            userDto.Roles = roles.Select(r => r.Name).ToList();

            var permissions = await _roleService.GetPermissionsForRoleAsync(roleIds: roles.Select(x => x.Id).ToList()); // فرض بر اینکه این متد وجود داره

            var accessToken = _jwtTokenGenerator.GenerateToken(
                userId: user.Id.ToString(),
                username: user.Username,
                roles: roles.Select(r => r.Name),
                permissions: permissions.Select(p => p.Name)
            );

            // 2. ساخت Refresh Token
            var refreshToken = GenerateSecureToken();

            var refreshTokenDto = new RefreshTokenDto
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            await _refreshTokenService.AddAsync(refreshTokenDto);

            // 3. خروجی نهایی
            return new LoginResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userDto,

            };
        }

        public virtual async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByExpressionAsync(u => u.Username == username);

            var userDto = _mapper.Map<UserDto>(user);

            if (user == null)
                throw new Exception("User not found");

            var userPassword = await _userPasswordService.GetPasswordByUserIdAsync(user.Id);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, userPassword.PasswordHash))
                throw new UnauthorizedAccessException("نام کاربری یا رمز عبور اشتباه است.");

            var roles = await _userService.GetRolesForUserAsync(user.Id); // فرض بر اینکه این متد وجود داره

            var permissions = await _roleService.GetPermissionsForRoleAsync(roleIds: roles.Select(x => x.Id).ToList()); // فرض بر اینکه این متد وجود داره

            var accessToken = _jwtTokenGenerator.GenerateToken(
                userId: user.Id.ToString(),
                username: user.Username,
                roles: roles.Select(r => r.Name),
                permissions: permissions.Select(p => p.Name)
            );


            return accessToken;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            // 1. بررسی اعتبار اولیه (اختیاری)
            var isValid = await _refreshTokenService.IsValidAsync(refreshToken);
            if (!isValid)
                throw new SecurityTokenException("توکن معتبر نیست یا قبلاً ابطال شده");

            // 2. ابطال توکن
            await _refreshTokenService.RevokeAsync(refreshToken);

        }

        public virtual async Task<UserDto> RegisterAsync(UserRegisterDto userDto)
        {
            var user = _mapper.Map<User>(userDto);

            var passSalt = _passwordHasher.GenerateSalt();

            var userPassword = new UserPasswordDto
            {
                UserId = user.Id,
                PasswordHash = _passwordHasher.HashPassword(userDto.Password),
            };

            await _userPasswordService.AddAsync(userPassword);

            //user.PasswordHash = _passwordHasher.HashPassword(userDto.Password);

            await _userRepository.AddAsync(user);

            return _mapper.Map<UserDto>(user);
        }


        #region Private Method
        private string GenerateSecureToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        #endregion
    }
}
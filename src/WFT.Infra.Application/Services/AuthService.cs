
using System.Security.Claims;
using System.Text;
using AutoMapper;
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

        public AuthService(IRepository<User> userRepository, IPasswordHasher passwordHasher,
                          IJwtTokenGenerator jwtTokenGenerator,
                          IMapper mapper, 
                          IRepository<Role> roleRepository,
                          IUserService userService,
                          IRoleService roleService
                          )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mapper = mapper;
            _roleRepository = roleRepository;
            _userService = userService; 
            _roleService = roleService;
        }

        public virtual async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByExpressionAsync(u => u.Username == username);

            var userDto = _mapper.Map<UserDto>(user);

            if (user == null)
                throw new Exception("User not found");

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedAccessException("نام کاربری یا رمز عبور اشتباه است.");

            var roles = await _userService.GetRolesForUserAsync(user.Id); // فرض بر اینکه این متد وجود داره

            var permissions = await _roleService.GetPermissionsForRoleAsync(roleIds: roles.Select(x=>x.Id).ToList()); // فرض بر اینکه این متد وجود داره

            var token = _jwtTokenGenerator.GenerateToken(
                userId: user.Id.ToString(),
                username: user.Username,
                roles: roles.Select(r => r.Name),
                permissions: permissions.Select(p => p.Name)
            );

            return token;
        }

        public virtual async Task<UserDto> RegisterAsync(UserDto userDto, string password)
        {
            var user = _mapper.Map<User>(userDto);

            // ?? ???? ?????
            user.PasswordHash = _passwordHasher.HashPassword(password);

            await _userRepository.AddAsync(user);

            return _mapper.Map<UserDto>(user);
        }
    }
}
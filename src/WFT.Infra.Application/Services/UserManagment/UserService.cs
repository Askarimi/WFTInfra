using AutoMapper;
using System.Linq.Expressions;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Application.Helper;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public partial class UserService : IUserService
    {

        #region ctor

        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;


        public UserService(IRepository<User> userRepository,
            IMapper mapper, 
            IRepository<UserRole> userRoleRepository, 
            IRepository<Role> roleRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService
            )
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;   
            _roleRepository = roleRepository;   
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }
        #endregion

        public Task AssignRolesAsync(long userId, List<long> roleIds)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<UserDto> AddAsync(UserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public virtual async Task DeleteAsync(long id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public virtual async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public virtual async Task<UserDto> GetByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public virtual async Task UpdateAsync(UserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _userRepository.UpdateAsync(user);
        }

        public virtual async Task<IEnumerable<RoleDto>> GetRolesForUserAsync(long userId)
        {
            // پیدا کردن تمام UserRole‌هایی که مربوط به این کاربر هستن
            var userRoles = await _userRoleRepository.GetListByExpressionAsync(ur => ur.UserId == userId);

            // استخراج RoleId‌ها
            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();

            if (!roleIds.Any())
                return Enumerable.Empty<RoleDto>();

            // پیدا کردن خود Roleها
            var roles = await _roleRepository.GetListByExpressionAsync(r => roleIds.Contains(r.Id));

            // تبدیل به DTO
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive
            });

            return roleDtos;
        }

        public async Task<bool> HasPermissionAsync(long userId, string permissionName)
        {
            var user = await _userRepository.GetWithIncludesAsync(
                u => u.Id == userId,
                u => u.UserRoles,
                u => u.UserRoles.Select(ur => ur.Role),
                u => u.UserRoles.Select(ur => ur.Role.RolePermissions),
                u => u.UserRoles.Select(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
            );

            var matchedUser = user.FirstOrDefault();
            if (matchedUser == null) return false;

            return matchedUser.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Any(rp => rp.Permission.Name == permissionName);
        }

        public async Task<long> RegisterByUserAsync(UserRegisterDto dto)
        {
            // چک کردن اینکه آیا کاربر قبلاً ثبت‌نام کرده یا نه

            // ایجاد یک شرط به صورت Expression
            var predicate = (Expression<Func<User, bool>>)(u => u.Email == dto.Email);

            var existingUser = await _userRepository.GetByExpressionAsync(predicate);
                

            if (existingUser != null)
                throw new Exception("User with this email already exists.");

            // ساختن کاربر جدید
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password), // هش کردن پسورد
                EmailConfirmed = false, // در این حالت کاربر باید ایمیل خودش رو تأیید کنه
                IsActive = false, // این مورد هم باید منتظر تأیید ایمیل بمونه
            };

            // اضافه کردن کاربر به دیتابیس
            await _userRepository.AddAsync(user);

            return user.Id;
        }

        public async Task<object> LoginAsync(UserLoginDto dto)
        {
            // بررسی وجود کاربر
            // ایجاد یک شرط به صورت Expression
            var predicate = (Expression<Func<User, bool>>)(u => u.Username == dto.UserName);
            var user = await _userRepository.GetByExpressionAsync(predicate);
                

            if (user == null)
                throw new Exception("کاربری با این ایمیل یافت نشد.");

            // بررسی رمز عبور
            var isPasswordValid = _passwordHasher.VerifyPassword(dto.Password,user.PasswordHash);

            if (!isPasswordValid)
                throw new Exception("رمز عبور اشتباه است.");

            if (!user.IsActive)
                throw new Exception("حساب کاربری غیرفعال است.");

            if (!user.EmailConfirmed)
                throw new Exception("ایمیل تأیید نشده است.");

            // تولید توکن
            var token = _tokenService.GenerateTokenForUser(user);

            // بازگرداندن اطلاعات کاربر و توکن
            return new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.Username,
                    user.Email
                }
            };
        }

    }
}

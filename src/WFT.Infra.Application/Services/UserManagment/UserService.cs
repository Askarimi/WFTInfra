using AutoMapper;
using System.Linq.Expressions;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Application.Contracts.Repositories;
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
        private readonly IUserPasswordService _userPasswordService;

        public UserService(IRepository<User> userRepository,
            IMapper mapper,
            IRepository<UserRole> userRoleRepository,
            IRepository<Role> roleRepository,
            IPasswordHasher passwordHasher,
            IUserPasswordService userPasswordService
            )
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _userPasswordService = userPasswordService;
        }
        #endregion

        public virtual async Task AddRoleToUserAsync(long userId, List<long> roleIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) throw new Exception("Role not found");

            var rolePermissions = roleIds.Select(roleId => new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            }).ToList();

            await _userRoleRepository.AddRangeAsync(rolePermissions);
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
        public async Task<IPagedList<UserDto>> GetPagedListAsync(PagedQueryRequest request)
        {

            // شروع از یک فیلتر پایه برای جستجوی عمومی
            Expression<Func<User, bool>> filter = user =>
                string.IsNullOrEmpty(request.SearchTerm) ||
                user.Username.Contains(request.SearchTerm) ||
                user.Email.Contains(request.SearchTerm) ||
                user.FirstName.Contains(request.SearchTerm) ||
                user.LastName.Contains(request.SearchTerm);

            // اگر فیلترهای اضافی (Filters) وجود دارند، آن‌ها را اضافه می‌کنیم
            if (request.Filters != null && request.Filters.Count > 0)
            {
                foreach (var filterItem in request.Filters)
                {
                    var property = typeof(User).GetProperty(filterItem.Key);
                    if (property != null)
                    {
                        var param = Expression.Parameter(typeof(User), "user");
                        var left = Expression.Property(param, property);
                        var right = Expression.Constant(filterItem.Value);
                        var equalExpression = Expression.Equal(left, right);

                        // ترکیب فیلترهای قبلی با فیلتر جدید
                        filter = Expression.Lambda<Func<User, bool>>(Expression.AndAlso(filter.Body, equalExpression), param);
                    }
                }
            }

            // دریافت داده‌ها با صفحه‌بندی
            var result = await _userRepository.GetPagedAsync(filter, request.PageNumber, request.PageSize);

            // تبدیل به UserDto
            var userDtos = result.Items.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt
            }).ToList();

            // بازگشت نتایج صفحه‌بندی‌شده
            return new PagedList<UserDto>(userDtos.AsQueryable(), result.TotalCount, result.PageNumber, result.PageSize);
        }

        public virtual async Task<UserDto> GetByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public virtual async Task<UserDto> GetByNameAsync(string name)
        {
            var user = await _userRepository.GetByExpressionAsync(u => u.FirstName.Contains(name) || u.LastName.Contains(name));

            return _mapper.Map<UserDto>(user);
        }

        public virtual async Task<UserDto> GetByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByExpressionAsync(u => u.Username.Contains(username));

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
                // PasswordHash = _passwordHasher.HashPassword(dto.Password), // هش کردن پسورد
                EmailConfirmed = false, // در این حالت کاربر باید ایمیل خودش رو تأیید کنه
                IsActive = false, // این مورد هم باید منتظر تأیید ایمیل بمونه

            };


            // اضافه کردن کاربر به دیتابیس
            await _userRepository.AddAsync(user);

            var userPassword = new UserPasswordDto
            {
                UserId = user.Id,

                PasswordHash = _passwordHasher.HashPassword(dto.Password),
            };

            await _userPasswordService.AddAsync(userPassword);

            return user.Id;
        }


    }
}

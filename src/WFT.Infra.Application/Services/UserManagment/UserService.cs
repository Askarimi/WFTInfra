using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
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

        private readonly IMapper _mapper;


        public UserService(IRepository<User> userRepository,
            IMapper mapper, 
            IRepository<UserRole> userRoleRepository, 
            IRepository<Role> roleRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;   
            _roleRepository = roleRepository;   
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


    }
}

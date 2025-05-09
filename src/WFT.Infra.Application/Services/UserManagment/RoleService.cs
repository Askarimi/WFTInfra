using AutoMapper;
using System.Linq.Expressions;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public partial class RoleService : IRoleService
    {
        #region ctor

        private readonly IRepository<Role> _roleRepository;

        private readonly IRepository<RolePermission> _rolePermissionRepository;

        private readonly IRepository<Permission> _permissionRepository;

        private readonly IMapper _mapper;

        public RoleService(IRepository<Role> roleRepository,
            IMapper mapper,
            IRepository<RolePermission> rolePermissionRepository,
            IRepository<Permission> permissionRepository)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
        }

        #endregion
        public async Task<RoleDto> GetByIdAsync(long id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> AddAsync(RoleDto roleDto)
        {
            var role = _mapper.Map<Role>(roleDto);
            await _roleRepository.AddAsync(role);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task UpdateAsync(RoleDto roleDto)
        {
            var role = _mapper.Map<Role>(roleDto);
            await _roleRepository.UpdateAsync(role);
        }

        public async Task DeleteAsync(long id)
        {
            await _roleRepository.DeleteAsync(id);
        }

        public virtual async Task AddPermissionsToRoleAsync(long roleId, List<long> permissionIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) throw new Exception("Role not found");

            var rolePermissions = permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            }).ToList();

            await _rolePermissionRepository.AddRangeAsync(rolePermissions);
        }

        public virtual async Task RemovePermissionsFromRoleAsync(long roleId, List<long> permissionIds)
        {

            // ایجاد یک شرط به صورت Expression
            var predicate = (Expression<Func<RolePermission, bool>>)(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId));

            var rolePermissions = await _rolePermissionRepository.GetListByExpressionAsync(predicate);

            await _rolePermissionRepository.RemoveRangeAsync(rolePermissions);
        }

        public virtual async Task<IEnumerable<PermissionDto>> GetPermissionsForRoleAsync(long roleId)
        {
            // ایجاد شرط داینامیک برای RolePermission
            var predicate = (Expression<Func<RolePermission, bool>>)(rp => rp.RoleId == roleId);

            // جستجو در RolePermission با استفاده از شرط داینامیک
            var rolePermissions = await _rolePermissionRepository.GetListByExpressionAsync(predicate);

            // استخراج PermissionIdها از نتایج RolePermission
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

            // اگر PermissionIds خالی بود، خروجی را برگردانید
            if (!permissionIds.Any())
                return Enumerable.Empty<PermissionDto>();

            // ایجاد شرط داینامیک برای پیدا کردن Permissions با Idهای خاص
            var permissionCondition = (Expression<Func<Permission, bool>>)(p => permissionIds.Contains(p.Id));

            // جستجو در Permission با استفاده از شرط داینامیک
            var permissions = await _permissionRepository.GetListByExpressionAsync(permissionCondition);

            // تبدیل Permissions به PermissionDto
            var permissionsDto = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName
            }).ToList(); // تبدیل به لیست برای بازگشت صحیح

            return permissionsDto;
        }

        public virtual async Task<IEnumerable<RoleDto>> GetActiveRolesAsync()
        {

            // ایجاد شرط داینامیک برای فیلتر کردن رول‌های فعال
            var predicate = (Expression<Func<Role, bool>>)(role => role.IsActive);

            // جستجو در Role با استفاده از شرط داینامیک
            var activeRoles = await _roleRepository.GetListByExpressionAsync(predicate);

            // تبدیل رول‌ها به RoleDto
            var roleDtos = activeRoles.Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive
            }).ToList(); // تبدیل به لیست برای بازگشت صحیح

            return roleDtos;
        }
    }
}

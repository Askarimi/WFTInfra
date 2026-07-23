using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement;
using WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.RolePermissionManagement
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRepository<RolePermission> _rolePermissionRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IMapper _mapper;

        public RolePermissionService(
            IRepository<RolePermission> rolePermissionRepository,
            IRepository<Role> roleRepository,
            IRepository<Permission> permissionRepository,
            IMapper mapper)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PermissionDetailDto>> GetPermissionsForRoleAsync(long roleId)
        {
            var roleExists = await _roleRepository.ExistsAsync(roleId);
            if (!roleExists)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            var rolePermissions = await _rolePermissionRepository.GetListByExpressionAsync(
                rp => rp.RoleId == roleId);

            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

            if (!permissionIds.Any())
                return Enumerable.Empty<PermissionDetailDto>();

            var permissions = await _permissionRepository.GetListByExpressionAsync(
                p => permissionIds.Contains(p.Id));

            return _mapper.Map<IEnumerable<PermissionDetailDto>>(permissions);
        }

        public async Task<bool> AssignPermissionsToRoleAsync(long roleId, long[] permissionIds)
        {
            var roleExists = await _roleRepository.ExistsAsync(roleId);
            if (!roleExists)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            if (!permissionIds.Any())
                return true;

            // Check all permissions exist
            foreach (var permissionId in permissionIds)
            {
                var exists = await _permissionRepository.ExistsAsync(permissionId);
                if (!exists)
                    throw new KeyNotFoundException($"Permission with ID {permissionId} not found");
            }

            // Get existing role permissions
            var existingRolePermissions = await _rolePermissionRepository.GetListByExpressionAsync(
                rp => rp.RoleId == roleId);
            var existingPermissionIds = existingRolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            // Create new role permissions for non-existing ones
            var newRolePermissions = new List<RolePermission>();
            foreach (var permissionId in permissionIds)
            {
                if (!existingPermissionIds.Contains(permissionId))
                {
                    newRolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    });
                }
            }

            if (newRolePermissions.Any())
                await _rolePermissionRepository.AddRangeAsync(newRolePermissions);

            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(long roleId, long permissionId)
        {
            var roleExists = await _roleRepository.ExistsAsync(roleId);
            if (!roleExists)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            var permissionExists = await _permissionRepository.ExistsAsync(permissionId);
            if (!permissionExists)
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found");

            var rolePermission = await _rolePermissionRepository.GetByExpressionAsync(
                rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
                return false;

            await _rolePermissionRepository.DeleteAsync(rolePermission.Id);
            return true;
        }

        public async Task<IEnumerable<RoleWithPermissionsDto>> GetAllRolesWithPermissionsAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            var result = new List<RoleWithPermissionsDto>();

            foreach (var role in roles)
            {
                var permissions = await GetPermissionsForRoleAsync(role.Id);
                var roleDto = _mapper.Map<RoleDetailDto>(role);

                result.Add(new RoleWithPermissionsDto
                {
                    Id = roleDto.Id,
                    Name = roleDto.Name,
                    Description = roleDto.Description,
                    IsActive = roleDto.IsActive,
                    Permissions = permissions.ToList()
                });
            }

            return result;
        }
    }
}


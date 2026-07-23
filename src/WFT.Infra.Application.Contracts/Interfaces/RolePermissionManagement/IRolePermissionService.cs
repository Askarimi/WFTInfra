using WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement;

namespace WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement
{
    public interface IRolePermissionService
    {
        Task<IEnumerable<PermissionDetailDto>> GetPermissionsForRoleAsync(long roleId);
        Task<bool> AssignPermissionsToRoleAsync(long roleId, long[] permissionIds);
        Task<bool> RemovePermissionFromRoleAsync(long roleId, long permissionId);
        Task<IEnumerable<RoleWithPermissionsDto>> GetAllRolesWithPermissionsAsync();
    }
}


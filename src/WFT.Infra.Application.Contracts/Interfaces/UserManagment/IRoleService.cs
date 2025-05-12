using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public partial interface IRoleService : IServiceBase<RoleDto>
    {
        Task AddPermissionsToRoleAsync(long roleId, List<long> permissionIds);
        Task RemovePermissionsFromRoleAsync(long roleId, List<long> permissionIds);
        Task<IEnumerable<PermissionDto>> GetPermissionsForRoleAsync(long roleId);
        Task<IEnumerable<PermissionDto>> GetPermissionsForRoleAsync(List<long> roleIds);
        Task<IEnumerable<RoleDto>> GetActiveRolesAsync();
    }
}

using WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement;

namespace WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionDetailDto>> GetAllPermissionsAsync();
        Task<PermissionDetailDto?> GetPermissionByIdAsync(long id);
        Task<PermissionDetailDto> CreatePermissionAsync(CreatePermissionDto dto);
        Task<PermissionDetailDto> UpdatePermissionAsync(UpdatePermissionDto dto);
        Task<bool> DeletePermissionAsync(long id);
    }
}


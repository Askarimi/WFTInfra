using WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement;

namespace WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDetailDto>> GetAllRolesAsync();
        Task<RoleDetailDto?> GetRoleByIdAsync(long id);
        Task<RoleDetailDto> CreateRoleAsync(CreateRoleDto dto);
        Task<RoleDetailDto> UpdateRoleAsync(UpdateRoleDto dto);
        Task<bool> DeleteRoleAsync(long id);
    }
}


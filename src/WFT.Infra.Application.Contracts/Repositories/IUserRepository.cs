using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Repositories
{
    public interface IUserRepository
    {
        Task<UserDto?> GetByIdWithRolesAndPermissionsAsync(long userId);
        Task<bool> HasPermissionAsync(long userId, string permissionName);
    }
}

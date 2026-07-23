using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public partial interface IUserService : IServiceBase<UserDto>
    {
        Task<IEnumerable<RoleDto>> GetRolesForUserAsync(long userId);

        Task<bool> HasPermissionAsync(long userId, string permissionName);

        Task<long> RegisterByUserAsync(UserRegisterDto dto);

        Task<UserDto> GetByUsernameAsync(string username);

        Task AddRoleToUserAsync(long userId, List<long> roleIds);
    }
}

using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public partial interface IUserService : IServiceBase<UserDto>
    {
        Task<IEnumerable<RoleDto>> GetRolesForUserAsync(long userId);

        Task<bool> HasPermissionAsync(long userId, string permissionName);

        Task<long> RegisterByUserAsync(UserRegisterDto dto);

        Task<object> LoginAsync(UserLoginDto dto);
    }
}

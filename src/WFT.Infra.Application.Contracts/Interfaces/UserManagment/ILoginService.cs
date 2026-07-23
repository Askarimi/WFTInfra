using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    /// <summary>
    /// Service for handling enhanced login with roles and permissions
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Login user and return response with roles and permissions
        /// </summary>
        Task<LoginResponseDto?> LoginWithRolesAndPermissionsAsync(UserLoginDto dto);

        /// <summary>
        /// Get aggregated permissions for a user from all assigned roles
        /// </summary>
        Task<IEnumerable<string>> GetUserPermissionsAsync(long userId);

        /// <summary>
        /// Get user's assigned roles
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(long userId);
    }
}


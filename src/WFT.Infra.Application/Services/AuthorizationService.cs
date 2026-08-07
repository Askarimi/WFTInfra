using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private const string SuperAdminRoleName = "SuperAdmin";

        private readonly IUserService _userService;
        private readonly IABACService _abacService;

        public AuthorizationService(
            IUserService userService,
            IABACService abacService,
            IRepository<User> userRepository)
        {
            _userService = userService;
            _abacService = abacService;
        }

        public async Task<bool> HasPermissionAsync(long userId, string permissionName, object? resource = null)
        {
            if (await IsSuperAdminAsync(userId))
                return true;

            // RBAC check
            var hasPermission = await _userService.HasPermissionAsync(userId, permissionName);
            if (!hasPermission) return false;

            // ABAC check
            if (resource != null)
            {
                return await _abacService.EvaluateAccessAsync(userId, permissionName, resource);
            }

            return true;
        }

        /// <summary>
        /// سطح Detailed برای گزارش و Debug
        /// </summary>
        public async Task<ABACEvaluationResponseDto> EvaluateAccessDetailedAsync(long userId, string permissionName, object? resource = null, object? context = null)
        {
            if (await IsSuperAdminAsync(userId))
            {
                return new ABACEvaluationResponseDto
                {
                    HasAccess = true,
                    UserId = userId,
                    Permission = permissionName,
                    Resource = resource,
                    Context = context,
                    EvaluationReason = "Access granted because the user has the SuperAdmin role.",
                    EvaluatedAt = DateTime.UtcNow
                };
            }

            return await _abacService.EvaluateAccessDetailedAsync(userId, permissionName, resource, context);
        }

        private async Task<bool> IsSuperAdminAsync(long userId)
        {
            var roles = await _userService.GetRolesForUserAsync(userId);

            return roles.Any(role =>
                string.Equals(role.Name, SuperAdminRoleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}

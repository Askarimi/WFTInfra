using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
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
            return await _abacService.EvaluateAccessDetailedAsync(userId, permissionName, resource, context);
        }
    }
}
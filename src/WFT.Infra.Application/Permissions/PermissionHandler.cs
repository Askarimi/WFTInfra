using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;

namespace WFT.Infra.Application.Permissions
{
    public partial class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserService _userService;
        private readonly IABACService _abacService;

        public PermissionHandler(IUserService userService, IABACService abacService)
        {
            _userService = userService;
            _abacService = abacService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                context.Fail();
                return;
            }

            var userId = long.Parse(userIdClaim.Value);

            // Check if permission requires ABAC evaluation
            var hasPermission = await _userService.HasPermissionAsync(userId, requirement.Permission);
            if (!hasPermission)
            {
                context.Fail();
                return;
            }

            // For now, we'll use basic RBAC. ABAC evaluation can be added later
            // when we have resource and context information available
            context.Succeed(requirement);
        }
    }
}

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
        private readonly IRepository<User> _userRepository;

        public AuthorizationService(
            IUserService userService,
            IABACService abacService,
            IRepository<User> userRepository)
        {
            _userService = userService;
            _abacService = abacService;
            _userRepository = userRepository;
        }

        public async Task<bool> CanCreateUserAsync(long currentUserId)
        {
            // RBAC Check: User must have CreateUser permission
            var hasPermission = await _userService.HasPermissionAsync(currentUserId, "CreateUser");
            if (!hasPermission)
                return false;

            // ABAC Check: Additional attribute-based rules can be added here
            // For example: Only users in HR department can create users
            var canCreate = await _abacService.EvaluateAccessAsync(
                currentUserId, 
                "CreateUser", 
                null, 
                null);

            return canCreate;
        }

        public async Task<bool> CanViewUserAsync(long currentUserId, long targetUserId)
        {
            // RBAC Check: User must have ViewUser permission
            var hasPermission = await _userService.HasPermissionAsync(currentUserId, "ViewUser");
            if (!hasPermission)
                return false;

            // ABAC Check: Users can only view users in the same department
            var canView = await _abacService.EvaluateAccessAsync(
                currentUserId, 
                "ViewUser", 
                targetUserId, 
                "User");

            return canView;
        }

        public async Task<bool> CanEditUserAsync(long currentUserId, long targetUserId)
        {
            // RBAC Check: User must have EditUser permission
            var hasPermission = await _userService.HasPermissionAsync(currentUserId, "EditUser");
            if (!hasPermission)
                return false;

            // ABAC Check: Users can only edit users in the same department
            var canEdit = await _abacService.EvaluateAccessAsync(
                currentUserId, 
                "EditUser", 
                targetUserId, 
                "User");

            return canEdit;
        }

        public async Task<bool> CanDeleteUserAsync(long currentUserId, long targetUserId)
        {
            // RBAC Check: User must have DeleteUser permission
            var hasPermission = await _userService.HasPermissionAsync(currentUserId, "DeleteUser");
            if (!hasPermission)
                return false;

            // ABAC Check: Users can only delete users they have created
            var canDelete = await _abacService.EvaluateAccessAsync(
                currentUserId, 
                "DeleteUser", 
                targetUserId, 
                "User");

            return canDelete;
        }

        public async Task<bool> CanViewUserListAsync(long currentUserId)
        {
            // RBAC Check: User must have ViewUser permission
            var hasPermission = await _userService.HasPermissionAsync(currentUserId, "ViewUser");
            if (!hasPermission)
                return false;

            // ABAC Check: Additional filtering rules can be applied
            var canViewList = await _abacService.EvaluateAccessAsync(
                currentUserId, 
                "ViewUserList", 
                null, 
                null);

            return canViewList;
        }

        public async Task<IEnumerable<long>> GetAccessibleUserIdsAsync(long currentUserId)
        {
            // Get all users first
            var allUsers = await _userRepository.GetAllAsync();
            var accessibleUsers = new List<long>();

            foreach (var user in allUsers)
            {
                if (await CanViewUserAsync(currentUserId, user.Id))
                {
                    accessibleUsers.Add(user.Id);
                }
            }

            return accessibleUsers;
        }

        public async Task<AuthorizationResult> ValidateUserAccessAsync(long currentUserId, long targetUserId, string operation)
        {
            var result = new AuthorizationResult();

            // Check if target user exists
            var targetUser = await _userRepository.GetByIdAsync(targetUserId);
            if (targetUser == null)
            {
                result.IsAuthorized = false;
                result.Reason = "کاربر مورد نظر یافت نشد.";
                return result;
            }

            // Check if current user exists
            var currentUser = await _userRepository.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                result.IsAuthorized = false;
                result.Reason = "کاربر جاری یافت نشد.";
                return result;
            }

            // RBAC Check
            var hasPermission = await _userService.HasPermissionAsync(currentUserId, operation);
            if (!hasPermission)
            {
                result.IsAuthorized = false;
                result.Reason = "شما مجوز انجام این عملیات را ندارید.";
                result.RequiredPermission = operation;
                return result;
            }

            // ABAC Check with detailed evaluation
            var abacResult = await _abacService.EvaluateAccessDetailedAsync(
                currentUserId, 
                operation, 
                targetUserId, 
                "User");

            if (!abacResult.HasAccess)
            {
                result.IsAuthorized = false;
                result.Reason = "دسترسی بر اساس ویژگی‌ها محدود شده است.";
                
                // Get the first failed policy for more details
                var failedPolicy = abacResult.PolicyResults?.FirstOrDefault(p => !p.IsApplicable || !p.ConditionsMet);
                if (failedPolicy != null)
                {
                    result.FailedAttribute = failedPolicy.PolicyName;
                    result.Reason = failedPolicy.Reason;
                }
                
                return result;
            }

            result.IsAuthorized = true;
            return result;
        }
    }
} 
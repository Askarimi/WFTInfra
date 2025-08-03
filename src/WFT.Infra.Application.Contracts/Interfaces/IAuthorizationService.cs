using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks if the current user can create new users
        /// </summary>
        Task<bool> CanCreateUserAsync(long currentUserId);

        /// <summary>
        /// Checks if the current user can view a specific user
        /// </summary>
        Task<bool> CanViewUserAsync(long currentUserId, long targetUserId);

        /// <summary>
        /// Checks if the current user can edit a specific user
        /// </summary>
        Task<bool> CanEditUserAsync(long currentUserId, long targetUserId);

        /// <summary>
        /// Checks if the current user can delete a specific user
        /// </summary>
        Task<bool> CanDeleteUserAsync(long currentUserId, long targetUserId);

        /// <summary>
        /// Checks if the current user can view the user list (with optional filtering)
        /// </summary>
        Task<bool> CanViewUserListAsync(long currentUserId);

        /// <summary>
        /// Gets the list of users that the current user can view
        /// </summary>
        Task<IEnumerable<long>> GetAccessibleUserIdsAsync(long currentUserId);

        /// <summary>
        /// Validates user access for a specific operation with detailed error information
        /// </summary>
        Task<AuthorizationResult> ValidateUserAccessAsync(long currentUserId, long targetUserId, string operation);
    }

    public class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string? Reason { get; set; }
        public string? RequiredPermission { get; set; }
        public string? FailedAttribute { get; set; }
        public object? ExpectedValue { get; set; }
        public object? ActualValue { get; set; }
    }
} 
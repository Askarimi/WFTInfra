using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(long userId, string permissionName, object? resource = null);

        /// <summary>
        /// بررسی دسترسی با جزئیات کامل (برای لاگ، Audit و Debug)
        /// </summary>
        Task<ABACEvaluationResponseDto> EvaluateAccessDetailedAsync(long userId, string permissionName, object? resource = null, object? context = null);

        // Task<IEnumerable<long>> GetAccessibleUserIdsAsync(long userId);
    }
}
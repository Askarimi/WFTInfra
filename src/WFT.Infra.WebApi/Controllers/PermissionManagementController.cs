using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement;
using WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement;
using WFT.Infra.WebApi.CustomConfig;

namespace WFT.Infra.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionManagementController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionManagementController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Get all permissions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> GetAllPermissions()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                return WFTJsonResult.Ok(permissions, message: "Permissions retrieved successfully");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error retrieving permissions", 500, ex);
            }
        }

        /// <summary>
        /// Get permission by ID
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> GetPermissionById(long id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);
                if (permission == null)
                    return WFTJsonResult.Fail($"Permission with ID {id} not found", 404);

                return WFTJsonResult.Ok(permission);
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error retrieving permission", 500, ex);
            }
        }

        /// <summary>
        /// Create new permission
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> CreatePermission([FromBody] CreatePermissionDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.DisplayName))
                    return WFTJsonResult.Fail("Permission name and display name are required", 400);

                var permission = await _permissionService.CreatePermissionAsync(dto);
                return WFTJsonResult.Ok(permission, message: "Permission created successfully");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error creating permission", 500, ex);
            }
        }

        /// <summary>
        /// Update existing permission
        /// </summary>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> UpdatePermission(long id, [FromBody] UpdatePermissionDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return WFTJsonResult.Fail("Permission ID mismatch", 400);

                if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.DisplayName))
                    return WFTJsonResult.Fail("Permission name and display name are required", 400);

                var permission = await _permissionService.UpdatePermissionAsync(dto);
                return WFTJsonResult.Ok(permission, message: "Permission updated successfully");
            }
            catch (KeyNotFoundException)
            {
                return WFTJsonResult.Fail($"Permission with ID {id} not found", 404);
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error updating permission", 500, ex);
            }
        }

        /// <summary>
        /// Delete permission
        /// </summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> DeletePermission(long id)
        {
            try
            {
                var success = await _permissionService.DeletePermissionAsync(id);
                if (!success)
                    return WFTJsonResult.Fail($"Permission with ID {id} not found", 404);

                return WFTJsonResult.Ok(message: "Permission deleted successfully");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error deleting permission", 500, ex);
            }
        }
    }
}


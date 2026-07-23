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
    public class RolePermissionController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;

        public RolePermissionController(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        [HttpGet("role/{roleId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> GetPermissionsForRole(long roleId)
        {
            try
            {
                var permissions = await _rolePermissionService.GetPermissionsForRoleAsync(roleId);
                return WFTJsonResult.Ok(permissions, message: "Permissions retrieved successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return WFTJsonResult.Fail(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error retrieving permissions for role", 500, ex);
            }
        }

        /// <summary>
        /// Assign one or more permissions to a role
        /// </summary>
        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> AssignPermissionsToRole([FromBody] AssignPermissionsDto dto)
        {
            try
            {
                if (dto.RoleId <= 0)
                    return WFTJsonResult.Fail("Invalid role ID", 400);

                if (!dto.PermissionIds.Any())
                    return WFTJsonResult.Fail("At least one permission ID is required", 400);

                await _rolePermissionService.AssignPermissionsToRoleAsync(dto.RoleId, dto.PermissionIds);
                return WFTJsonResult.Ok(message: "Permissions assigned to role successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return WFTJsonResult.Fail(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error assigning permissions to role", 500, ex);
            }
        }

        /// <summary>
        /// Remove a permission from a role
        /// </summary>
        [HttpPost("remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> RemovePermissionFromRole([FromBody] RemovePermissionDto dto)
        {
            try
            {
                if (dto.RoleId <= 0 || dto.PermissionId <= 0)
                    return WFTJsonResult.Fail("Invalid role or permission ID", 400);

                var success = await _rolePermissionService.RemovePermissionFromRoleAsync(dto.RoleId, dto.PermissionId);
                if (!success)
                    return WFTJsonResult.Fail("Permission was not assigned to this role", 404);

                return WFTJsonResult.Ok(message: "Permission removed from role successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return WFTJsonResult.Fail(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error removing permission from role", 500, ex);
            }
        }

        /// <summary>
        /// Get all roles with their permissions
        /// </summary>
        [HttpGet("with-permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> GetAllRolesWithPermissions()
        {
            try
            {
                var rolesWithPermissions = await _rolePermissionService.GetAllRolesWithPermissionsAsync();
                return WFTJsonResult.Ok(rolesWithPermissions, message: "Roles with permissions retrieved successfully");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error retrieving roles with permissions", 500, ex);
            }
        }
    }
}


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
    public class RoleManagementController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleManagementController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return WFTJsonResult.Ok(roles, message: "Roles retrieved successfully");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error retrieving roles", 500, ex);
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> GetRoleById(long id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    return WFTJsonResult.Fail($"Role with ID {id} not found", 404);

                return WFTJsonResult.Ok(role);
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error retrieving role", 500, ex);
            }
        }

        /// <summary>
        /// Create new role
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return WFTJsonResult.Fail("Role name is required", 400);

                var role = await _roleService.CreateRoleAsync(dto);
                return WFTJsonResult.Ok(role, message: "Role created successfully");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error creating role", 500, ex);
            }
        }

        /// <summary>
        /// Update existing role
        /// </summary>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> UpdateRole(long id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return WFTJsonResult.Fail("Role ID mismatch", 400);

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return WFTJsonResult.Fail("Role name is required", 400);

                var role = await _roleService.UpdateRoleAsync(dto);
                return WFTJsonResult.Ok(role, message: "Role updated successfully");
            }
            catch (KeyNotFoundException)
            {
                return WFTJsonResult.Fail($"Role with ID {id} not found", 404);
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error updating role", 500, ex);
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<WFTJsonResult> DeleteRole(long id)
        {
            try
            {
                var success = await _roleService.DeleteRoleAsync(id);
                if (!success)
                    return WFTJsonResult.Fail($"Role with ID {id} not found", 404);

                return WFTJsonResult.Ok(message: "Role deleted successfully");
            }
            catch (Exception ex)
            {
                return WFTJsonResult.Fail("Error deleting role", 500, ex);
            }
        }
    }
}


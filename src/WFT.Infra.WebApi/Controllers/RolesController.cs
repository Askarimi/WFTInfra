using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Core.Entities.UserManagment;


namespace WFT.Infra.WebApi.Controllers;

public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return await SuccessResponse(roles);
    }

    [HttpGet("{id:long}")]
    public override async Task<IActionResult> GetById(int id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null)
            return await ErrorResponse("Role not found", 404);

        return await SuccessResponse(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleDto roleDto)
    {
        var result = await _roleService.AddAsync(roleDto);

        return await CreatedResponse(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, RoleDto roleDto)
    {
       await _roleService.UpdateAsync( roleDto);
        

        return await NoContentResponse();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
         await _roleService.DeleteAsync(id);

        return await NoContentResponse();
    }
}

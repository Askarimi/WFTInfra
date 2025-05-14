using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Core.Entities.UserManagment;


namespace WFT.Infra.WebApi.Controllers;

public class PermissionssController : BaseController
{
    private readonly IPermissionService _permissionService;

    public PermissionssController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _permissionService.GetAllAsync();
        return await SuccessResponse(permissions);
    }

    [HttpGet("{id:long}")]
    public override async Task<IActionResult> GetById(int id)
    {
        var permission = await _permissionService.GetByIdAsync(id);
        if (permission == null)
            return await ErrorResponse("Permissions not found", 404);

        return await SuccessResponse(permission);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PermissionDto permissionDto)
    {
        var result = await _permissionService.AddAsync(permissionDto);

        return await CreatedResponse(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, PermissionDto permissionDto)
    {
       await _permissionService.UpdateAsync( permissionDto);
        

        return await NoContentResponse();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
         await _permissionService.DeleteAsync(id);

        return await NoContentResponse();
    }
}

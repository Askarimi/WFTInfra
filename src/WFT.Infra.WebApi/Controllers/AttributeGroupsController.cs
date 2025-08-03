using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class AttributeGroupsController : BaseController
    {
        private readonly IAttributeGroupService _attributeGroupService;
        private readonly IWorkContext _workContext;

        public AttributeGroupsController(IAttributeGroupService attributeGroupService, IWorkContext workContext)
        {
            _attributeGroupService = attributeGroupService;
            _workContext = workContext;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] AttributeGroupDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            request.CreatedUserId = _workContext.UserId.Value;

            var attributeGroup = await _attributeGroupService.AddAsync(request);
            var result = await _attributeGroupService.GetByIdAsync(attributeGroup.Id);

            return await CreatedResponse(nameof(GetById), new { id = attributeGroup.Id }, result);
        }

        // READ BY ID
        [HttpGet()]
        [Route("init/{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            var attributeGroup = await _attributeGroupService.GetByIdAsync(id);
            if (attributeGroup == null)
                return await ErrorResponse("گروه ویژگی یافت نشد.", 404);

            return await SuccessResponse(attributeGroup);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            if (request.PageNumber <= 0)
            {
                return BadRequest("PageNumber must be greater than 0");
            }

            if (request.PageSize <= 0)
            {
                return BadRequest("PageSize must be greater than 0");
            }

            var result = await _attributeGroupService.GetPagedListAsync(request);

            return await SuccessResponse(result);
        }

        // UPDATE
        [HttpPut()]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] AttributeGroupDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var result = await _attributeGroupService.UpdateAsync(request);

            return await SuccessResponse(result);
        }

        // DELETE
        [HttpDelete()]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _attributeGroupService.DeleteAsync(id);

            return await NoContentResponse();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var attributeGroup = await _attributeGroupService.GetByNameAsync(name);
            if (attributeGroup == null)
                return await ErrorResponse("گروه ویژگی یافت نشد.", 404);

            return await SuccessResponse(attributeGroup);
        }

        // Get active groups
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActiveGroups()
        {
            var groups = await _attributeGroupService.GetActiveGroupsAsync();

            return await SuccessResponse(groups);
        }

        // Get groups by sort order
        [HttpGet]
        [Route("sorted")]
        public async Task<IActionResult> GetGroupsBySortOrder()
        {
            var groups = await _attributeGroupService.GetGroupsBySortOrderAsync();

            return await SuccessResponse(groups);
        }

        // Assign attribute to group
        [HttpPost]
        [Route("assignattribute/{groupId:long}/{attributeId:long}")]
        public async Task<IActionResult> AssignAttributeToGroup(long groupId, long attributeId)
        {
            var success = await _attributeGroupService.AssignAttributeToGroupAsync(attributeId, groupId);
            if (!success)
                return await ErrorResponse("خطا در تخصیص ویژگی به گروه.");

            return await SuccessResponse(new { Success = true, Message = "ویژگی با موفقیت به گروه تخصیص داده شد." });
        }

        // Remove attribute from group
        [HttpDelete]
        [Route("removeattribute/{attributeId:long}")]
        public async Task<IActionResult> RemoveAttributeFromGroup(long attributeId)
        {
            var success = await _attributeGroupService.RemoveAttributeFromGroupAsync(attributeId);
            if (!success)
                return await ErrorResponse("خطا در حذف ویژگی از گروه.");

            return await NoContentResponse();
        }

        // Update sort order
        [HttpPut]
        [Route("sortorder/{groupId:long}")]
        public async Task<IActionResult> UpdateSortOrder(long groupId, [FromQuery] int sortOrder)
        {
            var success = await _attributeGroupService.UpdateSortOrderAsync(groupId, sortOrder);
            if (!success)
                return await ErrorResponse("خطا در بروزرسانی ترتیب گروه.");

            return await SuccessResponse(new { Success = true, Message = "ترتیب گروه با موفقیت بروزرسانی شد." });
        }
    }
} 
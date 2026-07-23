using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class AttributeGroupsController : BaseController
    {
        private readonly IAttributeGroupService _attributeGroupService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public AttributeGroupsController(
            IAttributeGroupService attributeGroupService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _attributeGroupService = attributeGroupService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] AttributeGroupDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "CreateAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "CreateAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ایجاد گروه ویژگی را ندارید.");
            }

            request.CreatedUserId = currentUserId;

            var attributeGroup = await _attributeGroupService.AddAsync(request);
            var result = await _attributeGroupService.GetByIdAsync(attributeGroup.Id);

            return CreatedAtAction(nameof(GetById), new { id = attributeGroup.Id }, result);
        }

        // READ BY ID
        [HttpGet]
        [Route("init/{id:long}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده گروه ویژگی را ندارید.");
            }

            var attributeGroup = await _attributeGroupService.GetByIdAsync(id);
            if (attributeGroup == null)
                return NotFound("گروه ویژگی یافت نشد.");

            return Ok(attributeGroup);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeGroupList");
            if (!authResult.HasAccess)
                return Forbid();

            if (request.PageNumber <= 0) return BadRequest("PageNumber must be greater than 0");
            if (request.PageSize <= 0) return BadRequest("PageSize must be greater than 0");

            var result = await _attributeGroupService.GetPagedListAsync(request);

            return PaginatedResponse<AttributeGroupDto>(result.Items, request.PageNumber, request.PageSize, result.TotalCount);
        }

        // UPDATE
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] AttributeGroupDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditAttributeGroup", request.Id);
            if (!authResult.HasAccess)
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش این گروه ویژگی را ندارید.");

            var result = await _attributeGroupService.UpdateAsync(request);

            return Ok(result);
        }

        // DELETE
        [HttpDelete]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "DeleteAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "DeleteAttributeGroup", id);
                return Forbid(authResult.EvaluationReason ?? "شما مجوز حذف این گروه ویژگی را ندارید.");
            }

            await _attributeGroupService.DeleteAsync(id);

            return NoContent();
        }

        // Get by name
        [HttpGet]
        [Route("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده گروه ویژگی را ندارید.");
            }

            var attributeGroup = await _attributeGroupService.GetByNameAsync(name);
            if (attributeGroup == null)
                return NotFound("گروه ویژگی یافت نشد.");

            return Ok(attributeGroup);
        }

        // Get active groups
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActiveGroups()
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده گروه ویژگی را ندارید.");
            }

            var groups = await _attributeGroupService.GetActiveGroupsAsync();

            return Ok(groups);
        }

        // Get groups by sort order
        [HttpGet]
        [Route("sorted")]
        public async Task<IActionResult> GetGroupsBySortOrder()
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده گروه ویژگی را ندارید.");
            }

            var groups = await _attributeGroupService.GetGroupsBySortOrderAsync();

            return Ok(groups);
        }

        // Assign attribute to group
        [HttpPost]
        [Route("assignattribute/{groupId:long}/{attributeId:long}")]
        public async Task<IActionResult> AssignAttributeToGroup(long groupId, long attributeId)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "EditAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش گروه ویژگی را ندارید.");
            }

            var success = await _attributeGroupService.AssignAttributeToGroupAsync(attributeId, groupId);
            if (!success)
                return BadRequest("خطا در تخصیص ویژگی به گروه.");

            return Ok(new { Success = true, Message = "ویژگی با موفقیت به گروه تخصیص داده شد." });
        }

        // Remove attribute from group
        [HttpDelete]
        [Route("removeattribute/{attributeId:long}")]
        public async Task<IActionResult> RemoveAttributeFromGroup(long attributeId)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "EditAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش گروه ویژگی را ندارید.");
            }

            var success = await _attributeGroupService.RemoveAttributeFromGroupAsync(attributeId);
            if (!success)
                return BadRequest("خطا در حذف ویژگی از گروه.");

            return NoContent();
        }

        // Update sort order
        [HttpPut]
        [Route("sortorder/{groupId:long}")]
        public async Task<IActionResult> UpdateSortOrder(long groupId, [FromQuery] int sortOrder)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "EditAttributeGroup");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EditAttributeGroup");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ویرایش گروه ویژگی را ندارید.");
            }

            var success = await _attributeGroupService.UpdateSortOrderAsync(groupId, sortOrder);
            if (!success)
                return BadRequest("خطا در بروزرسانی ترتیب گروه.");

            return Ok(new { Success = true, Message = "ترتیب گروه با موفقیت بروزرسانی شد." });
        }
    }
} 
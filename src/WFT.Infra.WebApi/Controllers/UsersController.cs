using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{

    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public UsersController(
            IUserService userService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _userService = userService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // CREATE
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Create([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            // Authorization Check: Can current user create users?
            var currentUserId = _workContext.UserId.Value;
            var canCreate = await _authorizationService.CanCreateUserAsync(currentUserId);
            if (!canCreate)
            {
                var authResult = await _authorizationService.ValidateUserAccessAsync(currentUserId, 0, "CreateUser");
                return await ErrorResponse(authResult.Reason ?? "شما مجوز ایجاد کاربر را ندارید.", 403);
            }

            request.CreatedUserId = currentUserId;

            var user = await _userService.AddAsync(request);
            var result = await _userService.GetByIdAsync(user.Id);

            return await CreatedResponse(nameof(GetById), new { id = user.Id }, result);
        }

        // READ BY ID
        [HttpGet()]
        [Route("init/{id:long}")]
        public override async Task<IActionResult> GetById(int id)
        {
            // Authorization Check: Can current user view this specific user?
            var currentUserId = _workContext.UserId.Value;
            var canView = await _authorizationService.CanViewUserAsync(currentUserId, id);
            if (!canView)
            {
                var authResult = await _authorizationService.ValidateUserAccessAsync(currentUserId, id, "ViewUser");
                return await ErrorResponse(authResult.Reason ?? "شما مجوز مشاهده این کاربر را ندارید.", 403);
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return await ErrorResponse("کاربر یافت نشد.", 404);

            return await SuccessResponse(user);
        }

        // READ ALL
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAll([FromQuery] PagedQueryRequest request)
        {
            // Authorization Check: Can current user view user list?
            var currentUserId = _workContext.UserId.Value;
            var canViewList = await _authorizationService.CanViewUserListAsync(currentUserId);
            if (!canViewList)
            {
                var authResult = await _authorizationService.ValidateUserAccessAsync(currentUserId, 0, "ViewUserList");
                return await ErrorResponse(authResult.Reason ?? "شما مجوز مشاهده لیست کاربران را ندارید.", 403);
            }

            // بررسی ورودی‌ها (اختیاری: می‌توانید اعتبارسنجی کنید که PageNumber و PageSize بزرگتر از صفر باشند)
            if (request.PageNumber <= 0)
            {
                return BadRequest("PageNumber must be greater than 0");
            }

            if (request.PageSize <= 0)
            {
                return BadRequest("PageSize must be greater than 0");
            }

            // Get accessible user IDs for filtering
            var accessibleUserIds = await _authorizationService.GetAccessibleUserIdsAsync(currentUserId);
            
            // Add filter to only show accessible users
            if (request.Filters == null)
                request.Filters = new Dictionary<string, object>();
            
            request.Filters["AccessibleUserIds"] = accessibleUserIds;

            // استفاده از سرویس برای دریافت داده‌ها
            var result = await _userService.GetPagedListAsync(request);

            // بازگشت نتیجه صفحه‌بندی شده
            return await SuccessResponse(result);
        }

        // UPDATE
        [HttpPut()]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            // Authorization Check: Can current user edit this specific user?
            var currentUserId = _workContext.UserId.Value;
            var canEdit = await _authorizationService.CanEditUserAsync(currentUserId, request.Id);
            if (!canEdit)
            {
                var authResult = await _authorizationService.ValidateUserAccessAsync(currentUserId, request.Id, "EditUser");
                return await ErrorResponse(authResult.Reason ?? "شما مجوز ویرایش این کاربر را ندارید.", 403);
            }

            var result = await _userService.UpdateAsync(request);

            return await SuccessResponse(result);
        }

        // DELETE
        [HttpDelete()]
        [Route("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            // Authorization Check: Can current user delete this specific user?
            var currentUserId = _workContext.UserId.Value;
            var canDelete = await _authorizationService.CanDeleteUserAsync(currentUserId, id);
            if (!canDelete)
            {
                var authResult = await _authorizationService.ValidateUserAccessAsync(currentUserId, id, "DeleteUser");
                return await ErrorResponse(authResult.Reason ?? "شما مجوز حذف این کاربر را ندارید.", 403);
            }

            await _userService.DeleteAsync(id);

            return await NoContentResponse();
        }
    }
}

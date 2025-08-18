using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class ABACController : BaseController
    {
        private readonly IABACService _abacService;
        private readonly IAttributeService _attributeService;
        private readonly IWorkContext _workContext;
        private readonly IAuthorizationService _authorizationService;

        public ABACController(
            IABACService abacService, 
            IAttributeService attributeService, 
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            _abacService = abacService;
            _attributeService = attributeService;
            _workContext = workContext;
            _authorizationService = authorizationService;
        }

        // Evaluate access (simple)
        [HttpPost]
        [Route("evaluate")]
        public async Task<IActionResult> EvaluateAccess([FromBody] ABACEvaluationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "EvaluateAccess");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EvaluateAccess");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ارزیابی دسترسی را ندارید.");
            }

            var hasAccess = await _abacService.EvaluateAccessAsync(
                request.UserId, 
                request.Permission, 
                request.Resource, 
                request.Context);

            return Ok(new { HasAccess = hasAccess });
        }

        // Evaluate access (detailed for frontend evaluation panel)
        [HttpPost]
        [Route("evaluate-detailed")]
        public async Task<IActionResult> EvaluateAccessDetailed([FromBody] ABACEvaluationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "EvaluateAccessDetailed");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "EvaluateAccessDetailed");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز ارزیابی تفصیلی دسترسی را ندارید.");
            }

            var evaluationResult = await _abacService.EvaluateAccessDetailedAsync(
                request.UserId, 
                request.Permission, 
                request.Resource, 
                request.Context);

            return Ok(evaluationResult);
        }

        // Get applicable policies for user and permission
        [HttpGet]
        [Route("policies/{userId:long}/{permission}")]
        public async Task<IActionResult> GetApplicablePolicies(long userId, string permission)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewPolicies");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewPolicies");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده سیاست‌ها را ندارید.");
            }

            var policies = await _abacService.GetApplicablePoliciesAsync(userId, permission);

            return Ok(policies);
        }

        // Get user attributes
        [HttpGet]
        [Route("userattributes/{userId:long}")]
        public async Task<IActionResult> GetUserAttributes(long userId)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewUserAttributes");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewUserAttributes");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده ویژگی‌های کاربر را ندارید.");
            }

            var attributes = await _abacService.GetUserAttributesAsync(userId);

            return Ok(attributes);
        }

        // Get specific attribute value for user
        [HttpGet]
        [Route("userattribute/{userId:long}/{attributeName}")]
        public async Task<IActionResult> GetUserAttributeValue(long userId, string attributeName)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewUserAttribute");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewUserAttribute");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده ویژگی کاربر را ندارید.");
            }

            var attributeValue = await _abacService.GetAttributeValueAsync(userId, attributeName);
            if (attributeValue == null)
                return NotFound("مقدار ویژگی یافت نشد.");

            return Ok(attributeValue);
        }

        // Set attribute value for user
        [HttpPost]
        [Route("setattribute")]
        public async Task<IActionResult> SetAttributeValue([FromBody] AttributeValueDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "SetAttributeValue");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "SetAttributeValue");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز تنظیم مقدار ویژگی را ندارید.");
            }

            var success = await _attributeService.SetAttributeValueAsync(request);
            if (!success)
                return BadRequest("خطا در تنظیم مقدار ویژگی.");

            return Ok(new { Success = true });
        }

        // Get resource attributes
        [HttpGet]
        [Route("resourceattributes/{resourceId:long}/{resourceType}")]
        public async Task<IActionResult> GetResourceAttributes(long resourceId, string resourceType)
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewResourceAttributes");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewResourceAttributes");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده ویژگی‌های منبع را ندارید.");
            }

            var attributes = await _attributeService.GetResourceAttributesAsync(resourceId, resourceType);

            return Ok(attributes);
        }

        // Validate policy rule
        [HttpPost]
        [Route("validatepolicy")]
        public async Task<IActionResult> ValidatePolicyRule([FromBody] PolicyRuleDto policyRule, [FromQuery] object? resource = null, [FromQuery] object? context = null)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ValidatePolicyRule");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ValidatePolicyRule");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز اعتبارسنجی قوانین سیاست را ندارید.");
            }

            var isValid = await _abacService.ValidatePolicyRuleAsync(policyRule, resource, context);

            return Ok(new { IsValid = isValid });
        }

        // Get evaluation statistics (for dashboard)
        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> GetEvaluationStats()
        {
            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "ViewEvaluationStats");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "ViewEvaluationStats");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز مشاهده آمار ارزیابی را ندارید.");
            }

            // This would typically come from a service that tracks evaluation metrics
            var stats = new
            {
                TotalEvaluations = 0, // TODO: Implement tracking
                SuccessfulEvaluations = 0,
                FailedEvaluations = 0,
                AverageEvaluationTime = 0.0,
                LastEvaluatedAt = DateTime.UtcNow
            };

            return Ok(stats);
        }

        // Bulk set user attributes
        [HttpPost]
        [Route("setuserattributes/{userId:long}")]
        public async Task<IActionResult> SetUserAttributes(long userId, [FromBody] List<AttributeValueDto> attributes)
        {
            if (!ModelState.IsValid)
                return BadRequest("اطلاعات وارد شده معتبر نیست.");

            var currentUserId = _workContext.UserId!.Value;

            var hasPermission = await _authorizationService.HasPermissionAsync(currentUserId, "SetUserAttributes");
            if (!hasPermission)
            {
                var authResult = await _authorizationService.EvaluateAccessDetailedAsync(currentUserId, "SetUserAttributes");
                return Forbid(authResult.EvaluationReason ?? "شما مجوز تنظیم ویژگی‌های کاربر را ندارید.");
            }

            var results = new List<object>();
            foreach (var attribute in attributes)
            {
                attribute.UserId = userId;
                var success = await _attributeService.SetAttributeValueAsync(attribute);
                results.Add(new { AttributeName = attribute.AttributeName, Success = success });
            }

            return Ok(results);
        }
    }
} 
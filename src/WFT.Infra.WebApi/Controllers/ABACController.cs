using Microsoft.AspNetCore.Mvc;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.WebApi.Controllers
{
    public class ABACController : BaseController
    {
        private readonly IABACService _abacService;
        private readonly IAttributeService _attributeService;
        private readonly IWorkContext _workContext;

        public ABACController(IABACService abacService, IAttributeService attributeService, IWorkContext workContext)
        {
            _abacService = abacService;
            _attributeService = attributeService;
            _workContext = workContext;
        }

        // Evaluate access (simple)
        [HttpPost]
        [Route("evaluate")]
        public async Task<IActionResult> EvaluateAccess([FromBody] ABACEvaluationRequestDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var hasAccess = await _abacService.EvaluateAccessAsync(
                request.UserId, 
                request.Permission, 
                request.Resource, 
                request.Context);

            return await SuccessResponse(new { HasAccess = hasAccess });
        }

        // Evaluate access (detailed for frontend evaluation panel)
        [HttpPost]
        [Route("evaluate-detailed")]
        public async Task<IActionResult> EvaluateAccessDetailed([FromBody] ABACEvaluationRequestDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var evaluationResult = await _abacService.EvaluateAccessDetailedAsync(
                request.UserId, 
                request.Permission, 
                request.Resource, 
                request.Context);

            return await SuccessResponse(evaluationResult);
        }

        // Get applicable policies for user and permission
        [HttpGet]
        [Route("policies/{userId:long}/{permission}")]
        public async Task<IActionResult> GetApplicablePolicies(long userId, string permission)
        {
            var policies = await _abacService.GetApplicablePoliciesAsync(userId, permission);

            return await SuccessResponse(policies);
        }

        // Get user attributes
        [HttpGet]
        [Route("userattributes/{userId:long}")]
        public async Task<IActionResult> GetUserAttributes(long userId)
        {
            var attributes = await _abacService.GetUserAttributesAsync(userId);

            return await SuccessResponse(attributes);
        }

        // Get specific attribute value for user
        [HttpGet]
        [Route("userattribute/{userId:long}/{attributeName}")]
        public async Task<IActionResult> GetUserAttributeValue(long userId, string attributeName)
        {
            var attributeValue = await _abacService.GetAttributeValueAsync(userId, attributeName);
            if (attributeValue == null)
                return await ErrorResponse("مقدار ویژگی یافت نشد.", 404);

            return await SuccessResponse(attributeValue);
        }

        // Set attribute value for user
        [HttpPost]
        [Route("setattribute")]
        public async Task<IActionResult> SetAttributeValue([FromBody] AttributeValueDto request)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var success = await _attributeService.SetAttributeValueAsync(request);
            if (!success)
                return await ErrorResponse("خطا در تنظیم مقدار ویژگی.");

            return await SuccessResponse(new { Success = true });
        }

        // Get resource attributes
        [HttpGet]
        [Route("resourceattributes/{resourceId:long}/{resourceType}")]
        public async Task<IActionResult> GetResourceAttributes(long resourceId, string resourceType)
        {
            var attributes = await _attributeService.GetResourceAttributesAsync(resourceId, resourceType);

            return await SuccessResponse(attributes);
        }

        // Validate policy rule
        [HttpPost]
        [Route("validatepolicy")]
        public async Task<IActionResult> ValidatePolicyRule([FromBody] PolicyRuleDto policyRule, [FromQuery] object? resource = null, [FromQuery] object? context = null)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var isValid = await _abacService.ValidatePolicyRuleAsync(policyRule, resource, context);

            return await SuccessResponse(new { IsValid = isValid });
        }

        // Get evaluation statistics (for dashboard)
        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> GetEvaluationStats()
        {
            // This would typically come from a service that tracks evaluation metrics
            var stats = new
            {
                TotalEvaluations = 0, // TODO: Implement tracking
                SuccessfulEvaluations = 0,
                FailedEvaluations = 0,
                AverageEvaluationTime = 0.0,
                LastEvaluatedAt = DateTime.UtcNow
            };

            return await SuccessResponse(stats);
        }

        // Bulk set user attributes
        [HttpPost]
        [Route("setuserattributes/{userId:long}")]
        public async Task<IActionResult> SetUserAttributes(long userId, [FromBody] List<AttributeValueDto> attributes)
        {
            if (!ModelState.IsValid)
                return await ErrorResponse("اطلاعات وارد شده معتبر نیست.");

            var results = new List<object>();
            foreach (var attribute in attributes)
            {
                attribute.UserId = userId;
                var success = await _attributeService.SetAttributeValueAsync(attribute);
                results.Add(new { AttributeName = attribute.AttributeName, Success = success });
            }

            return await SuccessResponse(results);
        }
    }
} 
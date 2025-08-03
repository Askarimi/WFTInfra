namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record PolicyRuleDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public string Effect { get; set; } = string.Empty;
        public List<PolicyConditionDto> PolicyConditions { get; set; } = new List<PolicyConditionDto>();
    }
} 
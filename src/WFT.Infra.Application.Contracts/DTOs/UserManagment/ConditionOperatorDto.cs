namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record ConditionOperatorDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string DataTypes { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
} 
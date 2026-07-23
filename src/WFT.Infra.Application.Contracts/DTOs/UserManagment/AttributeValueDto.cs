namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record AttributeValueDto : BaseDto
    {
        public long AttributeDefinitionId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public long? UserId { get; set; }
        public long? ResourceId { get; set; }
        public string? ResourceType { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }
} 
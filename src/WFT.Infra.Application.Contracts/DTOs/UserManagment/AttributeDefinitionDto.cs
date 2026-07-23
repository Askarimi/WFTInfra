namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record AttributeDefinitionDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public long? AttributeGroupId { get; set; }
        public string? AttributeGroupName { get; set; }
    }
} 
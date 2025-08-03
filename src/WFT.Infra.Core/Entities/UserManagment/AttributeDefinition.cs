namespace WFT.Infra.Core.Entities.UserManagment
{
    public class AttributeDefinition : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty; // "String", "Number", "Boolean", "DateTime"
        public string Source { get; set; } = string.Empty; // "User", "Resource", "Environment", "Action"
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public long? AttributeGroupId { get; set; }
        
        // Navigation Properties
        public virtual AttributeGroup? AttributeGroup { get; set; }
        public virtual ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
        public virtual ICollection<PolicyCondition> PolicyConditions { get; set; } = new List<PolicyCondition>();
    }
} 
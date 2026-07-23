namespace WFT.Infra.Core.Entities.UserManagment
{
    public class AttributeValue : BaseEntity
    {
        public long AttributeDefinitionId { get; set; }
        public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;
        
        public long? UserId { get; set; } // If Source = "User"
        public virtual User? User { get; set; }
        
        public long? ResourceId { get; set; } // If Source = "Resource"
        public string? ResourceType { get; set; } // "Document", "User", "Order"
        
        public string Value { get; set; } = string.Empty; // Value as JSON for complex types
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 
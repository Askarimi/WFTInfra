namespace WFT.Infra.Core.Entities.UserManagment
{
    public class AttributeGroup : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public string? Icon { get; set; }
        public string? Color { get; set; }

        // Navigation properties
        public virtual ICollection<AttributeDefinition> AttributeDefinitions { get; set; } = new List<AttributeDefinition>();
    }
} 
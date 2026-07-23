namespace WFT.Infra.Core.Entities.UserManagment
{
    public class PolicyCondition : BaseEntity
    {
        public long PolicyRuleId { get; set; }
        public virtual PolicyRule PolicyRule { get; set; } = null!;
        
        public long AttributeDefinitionId { get; set; }
        public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;
        
        public long ConditionOperatorId { get; set; }
        public virtual ConditionOperator ConditionOperator { get; set; } = null!;
        
        public string Value { get; set; } = string.Empty; // Comparison value
        public string LogicalOperator { get; set; } = "AND"; // "AND", "OR" (for combining conditions)
        public int Order { get; set; } = 1; // Execution order
    }
} 
namespace WFT.Infra.Core.Entities.UserManagment
{
    public class ConditionOperator : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // "Equals", "Contains", "GreaterThan"
        public string DisplayName { get; set; } = string.Empty; // "برابر است با", "شامل است", "بزرگتر از"
        public string Symbol { get; set; } = string.Empty; // "==", "contains", ">"
        public string DataTypes { get; set; } = string.Empty; // "String,Number" (JSON array)
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual ICollection<PolicyCondition> PolicyConditions { get; set; } = new List<PolicyCondition>();
    }
} 
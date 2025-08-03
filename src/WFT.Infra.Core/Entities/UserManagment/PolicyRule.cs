namespace WFT.Infra.Core.Entities.UserManagment
{
    public class PolicyRule : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // "DepartmentDocumentAccess"
        public string DisplayName { get; set; } = string.Empty; // "دسترسی به اسناد دپارتمان"
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 1; // Execution priority (1 = highest)
        public string Effect { get; set; } = string.Empty; // "Allow", "Deny"
        
        // Navigation Properties
        public virtual ICollection<PolicyCondition> PolicyConditions { get; set; } = new List<PolicyCondition>();
        public virtual ICollection<RolePolicyRule> RolePolicyRules { get; set; } = new List<RolePolicyRule>();
    }
} 
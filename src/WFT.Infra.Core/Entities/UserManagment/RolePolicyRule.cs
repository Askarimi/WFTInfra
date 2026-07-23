namespace WFT.Infra.Core.Entities.UserManagment
{
    public class RolePolicyRule : BaseEntity
    {
        public long RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
        
        public long PolicyRuleId { get; set; }
        public virtual PolicyRule PolicyRule { get; set; } = null!;
        
        public bool IsActive { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
} 
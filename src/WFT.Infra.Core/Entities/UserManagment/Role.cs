namespace WFT.Infra.Core.Entities.UserManagment
{
    public partial class Role : BaseEntity
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        
        public ICollection<RolePolicyRule> RolePolicyRules { get; set; } = new List<RolePolicyRule>();

    }
}

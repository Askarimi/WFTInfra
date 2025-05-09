namespace WFT.Infra.Core.Entities.UserManagment
{
    public abstract class User : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool EmailConfirmed { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

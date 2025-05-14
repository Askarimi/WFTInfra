namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record UserDto : BaseDto
    {
        public string FullName { set { value = $"{FirstName} {LastName}"; }}
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }
    }
}

namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record UserDto : BaseDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public List<string>? Roles { get; set; }
    }
}

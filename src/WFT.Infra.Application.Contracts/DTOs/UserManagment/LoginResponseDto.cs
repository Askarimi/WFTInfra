namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    /// <summary>
    /// Enhanced login response with roles and permissions
    /// </summary>
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public LoginUserDto User { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>
    /// User information in login response
    /// </summary>
    public class LoginUserDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}


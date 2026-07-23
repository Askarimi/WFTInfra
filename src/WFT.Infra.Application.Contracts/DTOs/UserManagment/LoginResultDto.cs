namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record LoginResultDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserDto User { get; set; } = null!;

    }
}

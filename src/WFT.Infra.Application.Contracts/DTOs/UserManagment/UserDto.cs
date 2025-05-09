namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record UserDto : BaseDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record UserPasswordDto
    {
        // کلید خارجی که به کاربر مربوطه اشاره می‌کند
        public long UserId { get; set; }

        // پسورد جدید
        public string Password { get; set; }

        // تایید پسورد
        public string ConfirmPassword { get; set; }
    }

    // DTO for authentication purposes (internal use)
    public record UserPasswordAuthDto
    {
        public long UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? LastChangedAt { get; set; }
    }
}

namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record UserPasswordDto
    {
        // کلید خارجی که به کاربر مربوطه اشاره می‌کند
        public long UserId { get; set; }

        // پسورد هش شده
        public string PasswordHash { get; set; }

        // تاریخ آخرین تغییر پسورد
        public DateTime? LastChangedAt { get; set; }
    }
}

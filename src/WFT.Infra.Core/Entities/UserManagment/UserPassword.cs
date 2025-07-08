namespace WFT.Infra.Core.Entities.UserManagment
{
    public partial class UserPassword : BaseEntity
    {
        // کلید خارجی که به کاربر مربوطه اشاره می‌کند
        public long UserId { get; set; }
        public User User { get; set; }
        // پسورد هش شده
        public string PasswordHash { get; set; }
        // تاریخ آخرین تغییر پسورد
        public DateTime? LastChangedAt { get; set; }
    }
}

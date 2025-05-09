namespace WFT.Infra.Core.Entities.UserManagment
{
    public abstract class Permission : BaseEntity
    {
        public string Name { get; set; }   // مثلاً: "EditUser"
        public string DisplayName { get; set; } // برای نمایش: "ویرایش کاربر"
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}

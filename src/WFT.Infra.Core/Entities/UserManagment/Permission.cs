namespace WFT.Infra.Core.Entities.UserManagment
{
    public partial class Permission : BaseEntity
    {
        public string Name { get; set; }   // مثلاً: "EditUser"
        public string DisplayName { get; set; } // برای نمایش: "ویرایش کاربر"
        public bool IsActive { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}

namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record PermissionDto : BaseDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public long RoleId { get; set; }
    }
}

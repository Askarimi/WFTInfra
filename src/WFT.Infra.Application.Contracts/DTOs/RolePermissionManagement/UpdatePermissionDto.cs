namespace WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement
{
    public class UpdatePermissionDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}


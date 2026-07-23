namespace WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement
{
    public class PermissionDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}


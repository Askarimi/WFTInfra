namespace WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement
{
    public class RoleWithPermissionsDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<PermissionDetailDto> Permissions { get; set; } = new();
    }
}


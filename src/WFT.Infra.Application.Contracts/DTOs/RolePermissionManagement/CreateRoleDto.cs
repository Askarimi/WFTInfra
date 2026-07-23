namespace WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement
{
    public class CreateRoleDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}


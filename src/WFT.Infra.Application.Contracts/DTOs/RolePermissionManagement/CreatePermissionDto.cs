namespace WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement
{
    public class CreatePermissionDto
    {
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}


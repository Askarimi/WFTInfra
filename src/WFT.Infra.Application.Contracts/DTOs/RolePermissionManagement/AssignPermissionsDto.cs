namespace WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement
{
    public class AssignPermissionsDto
    {
        public long RoleId { get; set; }
        public long[] PermissionIds { get; set; } = Array.Empty<long>();
    }
}


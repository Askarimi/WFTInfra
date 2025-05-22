namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public partial record RoleDto : BaseDto
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }
}

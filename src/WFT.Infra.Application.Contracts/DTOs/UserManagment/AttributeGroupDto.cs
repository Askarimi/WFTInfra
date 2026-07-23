using System.ComponentModel.DataAnnotations;

namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record AttributeGroupDto : BaseDto
    {
        [Required(ErrorMessage = "نام گروه ویژگی الزامی است")]
        [StringLength(100, ErrorMessage = "نام گروه ویژگی نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام نمایشی گروه ویژگی الزامی است")]
        [StringLength(200, ErrorMessage = "نام نمایشی گروه ویژگی نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [StringLength(50, ErrorMessage = "آیکون نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string? Icon { get; set; }

        [StringLength(20, ErrorMessage = "رنگ نمی‌تواند بیشتر از 20 کاراکتر باشد")]
        public string? Color { get; set; }

        // Additional properties for frontend
        public int AttributeCount { get; set; } = 0;
        public List<AttributeDefinitionDto> AttributeDefinitions { get; set; } = new List<AttributeDefinitionDto>();
    }
} 
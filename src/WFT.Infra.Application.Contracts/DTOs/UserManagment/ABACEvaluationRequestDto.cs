using System.ComponentModel.DataAnnotations;

namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record ABACEvaluationRequestDto
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public long UserId { get; set; }
        
        [Required(ErrorMessage = "دسترسی الزامی است")]
        [StringLength(100, ErrorMessage = "دسترسی نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Permission { get; set; } = string.Empty;
        
        public object? Resource { get; set; }
        public object? Context { get; set; }
    }
} 
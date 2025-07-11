using Portal.Shared.Enums.Support;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.ViewModel.Support
{
    public class SupportCategoryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "ประเภทหมวดหมู่")]
        [Required(ErrorMessage = "กรุณาเลือกประเภท")]
        public TicketCategoryType CategoryType { get; set; }

        [Display(Name = "ชื่อหมวดหมู่")]
        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "คำอธิบาย")]
        [MaxLength(255)]
        public string? Description { get; set; }
    }
}

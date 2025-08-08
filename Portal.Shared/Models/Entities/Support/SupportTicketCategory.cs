using Portal.Shared.Enums.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.Entities.Support
{
    public class SupportTicketCategory
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ประเภทหมวดหมู่")]
        public TicketCategoryType CategoryType { get; set; }

        [Display(Name = "ชื่อหมวดหมู่")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "คำอธิบาย")]
        [MaxLength(255)]
        public string? Description { get; set; }
        public bool IsNotCategory { get; set; }
    }
}

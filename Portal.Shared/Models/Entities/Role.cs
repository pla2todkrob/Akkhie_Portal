using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.Entities
{
    public class Role
    {
        public int Id { get; set; }

        [Display(Name = "บทบาท")]
        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "คำอธิบาย")]
        [MaxLength(500)]
        public string? Description { get; set; }
    }
}

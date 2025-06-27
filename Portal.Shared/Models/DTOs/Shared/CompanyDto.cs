using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Shared
{
    public class CompanyDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "ชื่อบริษัทจำเป็นต้องกรอก")]
        [StringLength(200, ErrorMessage = "ชื่อบริษัทต้องไม่เกิน 200 ตัวอักษร")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "รหัสบริษัทต้องไม่เกิน 50 ตัวอักษร")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "ประเภทบริษัทจำเป็นต้องกรอก")]
        public CompanyType Type { get; set; }

        [StringLength(100, ErrorMessage = "โดเมนต้องไม่เกิน 100 ตัวอักษร")]
        public string? Domain { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }

    public enum CompanyType
    {
        HeadOffice,
        Partner,
        Customer
    }
}

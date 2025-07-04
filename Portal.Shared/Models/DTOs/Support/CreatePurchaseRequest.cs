using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Support
{
    /// <summary>
    /// Represents a request to purchase a new IT item.
    /// </summary>
    public class CreatePurchaseRequest
    {
        [Required(ErrorMessage = "กรุณาระบุชื่ออุปกรณ์")]
        [MaxLength(200, ErrorMessage = "ชื่ออุปกรณ์ต้องไม่เกิน 200 ตัวอักษร")]
        [Display(Name = "ชื่ออุปกรณ์ที่ต้องการ")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุจำนวน")]
        [Range(1, 100, ErrorMessage = "จำนวนต้องอยู่ระหว่าง 1 ถึง 100")]
        [Display(Name = "จำนวน")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "สเปค / รายละเอียด (ถ้ามี)")]
        public string? Specification { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเหตุผลในการขอ")]
        [Display(Name = "เหตุผลในการขอ")]
        public string Reason { get; set; } = string.Empty;
    }
}

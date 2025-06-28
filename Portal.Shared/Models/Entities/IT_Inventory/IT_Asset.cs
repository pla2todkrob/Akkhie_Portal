using Portal.Shared.Enums.IT_Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.Entities.IT_Inventory
{
    public class IT_Asset
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "เลขทรัพย์สิน")]
        [MaxLength(100)]
        public string AssetTag { get; set; } = string.Empty;

        [Display(Name = "ชนิดอุปกรณ์")]
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        public IT_Item Item { get; set; } = null!;

        [Display(Name = "Serial Number")]
        [MaxLength(200)]
        public string? SerialNumber { get; set; }

        [Display(Name = "ผู้ครอบครอง")]
        public Guid? AssignedToEmployeeId { get; set; }

        [ForeignKey("AssignedToEmployeeId")]
        public Employee? AssignedToEmployee { get; set; }

        [Display(Name = "วันที่ซื้อ")]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "สถานะ")]
        public AssetStatus Status { get; set; } = AssetStatus.InStock;
    }
}

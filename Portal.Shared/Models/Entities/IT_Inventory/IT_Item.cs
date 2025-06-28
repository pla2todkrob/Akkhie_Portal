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
    public class IT_Item
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ชื่ออุปกรณ์/รุ่น")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "ประเภท")]
        public ItemType ItemType { get; set; }

        [Display(Name = "มีในสต็อก")]
        public bool IsStockItem { get; set; }

        [Display(Name = "รายละเอียด/สเปค")]
        [Column(TypeName = "nvarchar(max)")]
        public string? Specification { get; set; } // Store as JSON
    }
}

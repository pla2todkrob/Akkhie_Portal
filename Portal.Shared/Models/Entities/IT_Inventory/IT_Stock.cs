using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.Entities.IT_Inventory
{
    public class IT_Stock
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ชนิดอุปกรณ์")]
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        public IT_Item Item { get; set; } = null!;

        [Display(Name = "จำนวนคงเหลือ")]
        public int Quantity { get; set; }

        [Display(Name = "ตำแหน่งที่เก็บ")]
        [MaxLength(100)]
        public string? Location { get; set; }
    }
}

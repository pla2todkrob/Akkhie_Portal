using Portal.Shared.Enums.IT_Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.ViewModel.IT_Inventory
{
    public class StockItemViewModel
    {
        public int StockId { get; set; }
        public int ItemId { get; set; }

        [Display(Name = "อุปกรณ์")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "ประเภท")]
        public ItemType ItemType { get; set; }

        [Display(Name = "รายละเอียด")]
        public string? Specification { get; set; }

        [Display(Name = "คงเหลือ")]
        public int Quantity { get; set; }
    }
}

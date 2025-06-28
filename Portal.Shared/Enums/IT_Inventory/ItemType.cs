using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Enums.IT_Inventory
{
    public enum ItemType
    {
        [Display(Name = "อุปกรณ์สิ้นเปลือง")]
        Consumable = 1,

        [Display(Name = "ทรัพย์สิน")]
        Asset = 2
    }
}

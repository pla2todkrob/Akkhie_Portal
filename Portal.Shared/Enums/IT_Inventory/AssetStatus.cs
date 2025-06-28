using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Enums.IT_Inventory
{
    public enum AssetStatus
    {
        [Display(Name = "ในสต็อก")]
        InStock = 1,

        [Display(Name = "ใช้งานอยู่")]
        InUse = 2,

        [Display(Name = "กำลังซ่อม")]
        Repairing = 3,

        [Display(Name = "จำหน่าย/ตัดบัญชี")]
        Retired = 4
    }
}

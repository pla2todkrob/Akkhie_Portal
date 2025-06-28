using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Enums.Support
{
    public enum TicketStatus
    {
        [Display(Name = "เปิด")]
        Open = 1,

        [Display(Name = "กำลังดำเนินการ")]
        InProgress = 2,

        [Display(Name = "แก้ไขแล้ว")]
        Resolved = 3,

        [Display(Name = "ปิด")]
        Closed = 4
    }
}

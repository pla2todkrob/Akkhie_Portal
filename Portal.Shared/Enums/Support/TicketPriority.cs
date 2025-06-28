using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Enums.Support
{
    public enum TicketPriority
    {
        [Display(Name = "ต่ำ")]
        Low = 0,

        [Display(Name = "ปกติ")]
        Medium = 1,

        [Display(Name = "สูง")]
        High = 2,

        [Display(Name = "เร่งด่วน")]
        Critical = 3
    }
}

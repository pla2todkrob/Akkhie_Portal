using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Enums.Support
{
    public enum TicketCategoryType
    {
        [Display(Name = "ปัญหา")]
        Issue = 1,

        [Display(Name = "คำขอ")]
        Request = 2
    }
}

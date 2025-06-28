using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Enums.Support
{
    public enum TicketRequestType
    {
        [Display(Name = "แจ้งปัญหา")]
        Issue = 1,

        [Display(Name = "เบิก/ขออุปกรณ์")]
        Request = 2
    }
}

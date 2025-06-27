using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Enums
{
    public enum EmployeeStatus
    {
        [Display(Name = "ไม่พร้อมใช้งาน")]
        Inactive = 0,

        [Display(Name = "พร้อมใช้งาน")]
        Active = 1,

        [Display(Name = "ถูกระงับ")]
        Suspended = 2,

        [Display(Name = "ยกเลิกใช้งาน")]
        Cancelled = 3
    }
}

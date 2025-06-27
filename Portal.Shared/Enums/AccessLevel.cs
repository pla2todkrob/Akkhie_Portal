using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Enums
{
    public enum AccessLevel
    {
        [Display(Name = "ไม่มีสิทธ์")]
        None = 0,

        [Display(Name = "อ่านเท่านั้น")]
        Read = 1,

        [Display(Name = "อ่านและเขียน")]
        Write = 2,

        [Display(Name = "ผู้ดูแล")]
        Admin = 3
    }
}

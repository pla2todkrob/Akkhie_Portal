// RoleType.cs ในโฟลเดอร์ Enums
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Enums
{
    public enum RoleType
    {
        [Display(Name = "ประธานกรรมการบริหาร")]
        Chairman = 1,

        [Display(Name = "กรรมการผู้จัดการ")]
        ManagingDirector = 2,

        [Display(Name = "เลขานุการ")]
        Secretary = 3,

        [Display(Name = "รองกรรมการผู้จัดการ")]
        DeputyManagingDirector = 4,

        [Display(Name = "ผู้จัดการฝ่าย")]
        DepartmentManager = 5,

        [Display(Name = "หัวหน้าแผนก")]
        SectionManager = 6,

        [Display(Name = "เจ้าหน้าที่ทั่วไป")]
        Staff = 7
    }
}

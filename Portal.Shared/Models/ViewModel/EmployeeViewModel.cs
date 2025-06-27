using Portal.Shared.Enums;
using Portal.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.ViewModel
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "ชื่อบัญชี")]
        public string Username { get; set; }

        [Display(Name = "ผู้ใช้ AD")]
        public bool IsAdUser { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "สายงาน")]
        public string? DivisionName { get; set; }

        [Display(Name = "ฝ่าย")]
        public string? DepartmentName { get; set; }

        [Display(Name = "แผนก")]
        public string? SectionName { get; set; }

        [Display(Name = "บทบาท")]
        public string RoleName { get; set; }

        public bool IsSystemRole { get; set; }

        [DisplayFormat(DataFormatString = "{0:g}")]
        [Display(Name = "วันที่สร้าง")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "สถานะ")]
        public EmployeeStatus EmployeeStatus { get; set; }

        [Display(Name = "รหัสพนักงาน")]
        public string EmployeeCode { get; set; }

        [Display(Name = "ชื่อจริงภาษาอังกฤษ")]
        public string FirstName { get; set; }

        [Display(Name = "นามสกุลภาษาอังกฤษ")]
        public string LastName { get; set; }

        [Display(Name = "ชื่อจริงภาษาท้องถิ่น")]
        public string LocalFirstName { get; set; }

        [Display(Name = "นามสกุลภาษาท้องถิ่น")]
        public string LocalLastName { get; set; }

        [Display(Name = "อีเมล")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "ชื่อภาษาอังกฤษ")]
        public string FullName { get; set; }

        [Display(Name = "ชื่อภาษาท้องถิ่น")]
        public string LocalFullName { get; set; }

        [DataType(DataType.Url)]
        [Display(Name = "โปรไฟล์")]
        public string ProfileUrl { get; set; }
    }
}

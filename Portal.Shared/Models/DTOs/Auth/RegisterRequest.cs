// Portal.Shared/Models/DTOs/Auth/RegisterRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Auth
{
    public class RegisterRequest : IValidatableObject
    {
        public bool IsAdUser { get; set; } = false;

        [Display(Name = "ชื่อผู้ใช้")]
        [Required(ErrorMessage = "กรุณากรอก Username")]
        [StringLength(255, ErrorMessage = "Username ต้องไม่เกิน 255 ตัวอักษร")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "รหัสพนักงาน")]
        [Required(ErrorMessage = "กรุณากรอกรหัสพนักงาน")]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Display(Name = "ชื่อจริงภาษาอังกฤษ")]
        [Required(ErrorMessage = "กรุณากรอกชื่อจริงภาษาอังกฤษ")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "นามสกุลภาษาอังกฤษ")]
        [Required(ErrorMessage = "กรุณากรอกนามสกุลภาษาอังกฤษ")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "ชื่อจริงภาษาท้องถิ่น")]
        [Required(ErrorMessage = "กรุณากรอกชื่อจริงภาษาท้องถิ่น")]
        [StringLength(100)]
        public string LocalFirstName { get; set; } = string.Empty;

        [Display(Name = "นามสกุลภาษาท้องถิ่น")]
        [Required(ErrorMessage = "กรุณากรอกนามสกุลภาษาท้องถิ่น")]
        [StringLength(100)]
        public string LocalLastName { get; set; } = string.Empty;

        [Display(Name = "อีเมล")]
        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "รหัสผ่าน")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "ยืนยันรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "รหัสผ่านและการยืนยันรหัสผ่านไม่ตรงกัน")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        [MaxLength(20, ErrorMessage = "เบอร์โทรศัพท์ต้องไม่เกิน 20 ตัวอักษร")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "สายงาน")]
        public int? DivisionId { get; set; }

        [Display(Name = "ฝ่าย")]
        public int? DepartmentId { get; set; }

        [Display(Name = "แผนก")]
        public int? SectionId { get; set; }

        [Display(Name = "บทบาท")]
        public int? RoleId { get; set; }

        [Display(Name = "สิทธิ์ระดับระบบ")]
        public bool IsSystemRole { get; set; } = false;

        public string? ReturnUrl { get; set; }

        [Display(Name = "บริษัท")]
        public int? CompanyId { get; set; }

        [Display(Name = "สาขา")]
        public int? CompanyBranchId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsAdUser)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    yield return new ValidationResult(
                        "กรุณากรอกรหัสผ่าน",
                        [nameof(Password)]);
                }

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    yield return new ValidationResult(
                        "กรุณายืนยันรหัสผ่าน",
                        [nameof(ConfirmPassword)]);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    yield return new ValidationResult(
                        "ผู้ใช้ AD ไม่จำเป็นต้องกำหนดรหัสผ่านในการลงทะเบียน",
                        [nameof(Password), nameof(ConfirmPassword)]);
                }
            }
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Auth
{
    public class LoginRequest
    {
        [Display(Name = "ชื่อผู้ใช้")]
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        [StringLength(255, ErrorMessage = "ชื่อผู้ใช้ต้องไม่เกิน 255 ตัวอักษร")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [Display(Name = "รหัสผ่าน")]
        [DataType(DataType.Password)]
        [StringLength(255, ErrorMessage = "รหัสผ่านต้องไม่เกิน 255 ตัวอักษร")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "จำฉันไว้")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}

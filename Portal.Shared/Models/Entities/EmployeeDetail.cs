// Portal.Shared/Models/Entities/EmployeeDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities
{
    public class EmployeeDetail
    {
        [Key]
        [ForeignKey("Employee")]
        public Guid EmployeeId { get; set; }

        [MaxLength(50)]
        [Required]
        public string EmployeeCode { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string LocalFirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string LocalLastName { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(255)]
        [Required]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public Employee Employee { get; set; } = null!;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string LocalFullName => $"{LocalFirstName} {LocalLastName}";
    }
}

using Portal.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Shared
{
    public class UpdateStatusRequestDto
    {
        [Required]
        [EnumDataType(typeof(EmployeeStatus))]
        public EmployeeStatus NewStatus { get; set; }
    }
}

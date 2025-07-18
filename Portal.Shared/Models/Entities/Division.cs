using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.Entities
{
    public class Division
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public ICollection<Department> Departments { get; set; } = [];
    }
}

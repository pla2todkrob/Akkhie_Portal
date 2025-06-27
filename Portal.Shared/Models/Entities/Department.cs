using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities
{
    public class Department
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        public int DivisionId { get; set; }

        [ForeignKey("DivisionId")]
        public Division Division { get; set; } = null!;

        public ICollection<Section> Sections { get; set; } = [];
    }
}

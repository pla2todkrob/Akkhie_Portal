using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.Entities
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Key { get; set; }

        public string? Description { get; set; }
    }
}
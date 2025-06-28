using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.Entities.IT_Inventory
{
    public class IT_StandardSet
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ชื่อชุดอุปกรณ์")]
        [Required]
        [MaxLength(150)]
        public string SetName { get; set; } = string.Empty;

        [Display(Name = "ชนิดอุปกรณ์")]
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        public IT_Item Item { get; set; } = null!;

        [Display(Name = "สำหรับ Role")]
        public int AssignedToRoleId { get; set; }

        [ForeignKey("AssignedToRoleId")]
        public Role AssignedToRole { get; set; } = null!;
    }
}

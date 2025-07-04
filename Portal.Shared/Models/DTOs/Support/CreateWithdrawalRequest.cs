using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Shared.Models.DTOs.Support
{
    public class CreateWithdrawalRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "กรุณาเลือกอุปกรณ์อย่างน้อย 1 รายการ")]
        public List<CartItemDto> Items { get; set; } = [];
    }

    /// <summary>
    /// Represents a single item within the withdrawal cart.
    /// </summary>
    public class CartItemDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "จำนวนต้องมากกว่า 0")]
        public int Quantity { get; set; }
    }
}

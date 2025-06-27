using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace FRPAMSystem.BusinessTier.Payload.Email
{
    public class TestEmailRequest
    {
        /// <summary>Email người nhận — không cần có trong bảng User.</summary>
        [Required]
        [EmailAddress]
        public string ToEmail { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;
    }
}

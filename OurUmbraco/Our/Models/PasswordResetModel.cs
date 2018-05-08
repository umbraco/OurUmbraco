using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OurUmbraco.Our.Models
{
    public class PasswordResetModel
    {
        [Required(ErrorMessage = "Please enter your email address")]
        [DisplayName("Email*")]
        public string Email { get; set; }

        public string Token { get; set; }

        [Required(ErrorMessage = "Please enter a new password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [DisplayName("New password*")]
        public string NewPassword { get; set; }
    }
}

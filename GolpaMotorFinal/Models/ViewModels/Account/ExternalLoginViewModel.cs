using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}

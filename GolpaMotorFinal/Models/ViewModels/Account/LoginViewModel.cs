using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "رمز ورود")]
        public string Password { get; set; } = null!;

        [Display(Name = "مرا به خاطر بسپار")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}

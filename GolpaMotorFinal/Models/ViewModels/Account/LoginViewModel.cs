using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        
        [EmailAddress]
        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = "ایمیل الزامی است")]
        public string Email { get; set; } = null!;

       
        [DataType(DataType.Password)]
        [Display(Name = "رمز ورود")]
        [Required(ErrorMessage = "رمز ورود الزامی است")]
        public string Password { get; set; } = null!;

        [Display(Name = "مرا به خاطر بسپار")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}

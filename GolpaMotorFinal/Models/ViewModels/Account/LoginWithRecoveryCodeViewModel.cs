using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class LoginWithRecoveryCodeViewModel
    {
        [Required]
        [Display(Name = "Recovery code")]
        public string RecoveryCode { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}

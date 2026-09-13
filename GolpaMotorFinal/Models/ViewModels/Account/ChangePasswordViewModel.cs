using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "رمز فعلی را وارد کنید.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز فعلی")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز جدید را وارد کنید.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز جدید")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تکرار رمز جدید را وارد کنید.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "رمز جدید و تکرار آن یکسان نیست.")]
        [Display(Name = "تکرار رمز جدید")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

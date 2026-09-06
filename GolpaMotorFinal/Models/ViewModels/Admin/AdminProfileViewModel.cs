using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GolpaMotorFinal.Models.ViewModels.Admin
{
    public class AdminProfileViewModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}

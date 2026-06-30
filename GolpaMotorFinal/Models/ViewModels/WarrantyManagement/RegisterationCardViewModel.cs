using Framework.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.WarrantyManagement
{
    public class RegisterationCardViewModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "شماره سریال باید بین ۳ تا 50 کاراکتر باشد.")]
        [Required(ErrorMessage = "شماره سریال اجباری است.")]
        [Display(Name = "شماره سریال")]
        public List<string> SerialNumber { get; set; } = new List<string>();

        [StringLength(50, MinimumLength = 3, ErrorMessage = "رمز باید بین ۳ تا 50 کاراکتر باشد.")]
        [Required(ErrorMessage = "رمز اجباری است.")]
        [Display(Name = "رمز")]
        public List<string> ScratchedCode { get; set; } = new List<string>();

        [StringLength(50, MinimumLength = 3, ErrorMessage = "شماره موبایل باید بین ۳ تا 50 کاراکتر باشد.")]
        [Required(ErrorMessage = "شماره موبایل اجباری است.")]
        [Display(Name = "شماره موبایل")]
        public string CustomerPhoneNumber { get; set; }
        public OperationResult? op { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required(ErrorMessage = "لطفا نقش خود را انتخاب کنید.")]
        public int? CustomerTypeId { get; set; }
        public IEnumerable<SelectListItem> CustomerTypes { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace DomainModel.ViewModels.Reward
{
    public class RewardCatalogAddEditModel
    {
        public int RewardCatalogID { get; set; }

        [Required(ErrorMessage = "عنوان پاداش اجباری است.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "عنوان باید بین ۲ تا ۱۰۰ کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [StringLength(400, ErrorMessage = "توضیحات حداکثر ۴۰۰ کاراکتر است.")]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "حد نصاب امتیاز اجباری است.")]
        [Range(1, int.MaxValue, ErrorMessage = "حد نصاب باید بزرگ‌تر از صفر باشد.")]
        [Display(Name = "حد نصاب امتیاز")]
        public int RequiredPoints { get; set; }

        [Display(Name = "پاداش نقدی")]
        public bool IsCashReward { get; set; }

        [Display(Name = "مبلغ نقدی")]
        public decimal? CashValue { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }
}

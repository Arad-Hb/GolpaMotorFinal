using System.ComponentModel.DataAnnotations;

namespace DomainModel.ViewModels.Reward
{
    public class RewardCatalogListItem
    {
        public int RewardCatalogID { get; set; }

        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "حد نصاب امتیاز")]
        public int RequiredPoints { get; set; }

        [Display(Name = "پاداش نقدی")]
        public bool IsCashReward { get; set; }

        [Display(Name = "مبلغ نقدی")]
        public decimal? CashValue { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        public int RequestCount { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace DomainModel.ViewModels.Reward
{
    public class RewardRequestListItem
    {
        public int RewardRequestID { get; set; }

        public string UserID { get; set; } = string.Empty;

        [Display(Name = "کاربر")]
        public string UserFullName { get; set; } = string.Empty;

        [Display(Name = "موبایل")]
        public string? PhoneNumber { get; set; }

        public int RewardCatalogID { get; set; }

        [Display(Name = "پاداش")]
        public string CatalogTitle { get; set; } = string.Empty;

        [Display(Name = "حد نصاب")]
        public int RequiredPoints { get; set; }

        [Display(Name = "مانده امتیاز")]
        public int RemainedPoints { get; set; }

        public DateTime? RequestDate { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public bool IsComplete { get; set; }

        public int RewardDeliveryStatusID { get; set; }

        [Display(Name = "وضعیت")]
        public string StatusTitle { get; set; } = string.Empty;
    }
}

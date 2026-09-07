using Framework.Common;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.ViewModels.Reward
{
    public class RewardCatalogSearchModel : PageModel
    {
        [Display(Name = "عنوان")]
        public string? Title { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsCashReward { get; set; }
    }
}

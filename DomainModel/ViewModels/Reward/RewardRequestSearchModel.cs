using Framework.Common;

namespace DomainModel.ViewModels.Reward
{
    public class RewardRequestSearchModel : PageModel
    {
        public string? SearchTerm { get; set; }
        public int? RewardDeliveryStatusID { get; set; }
        public bool? IsComplete { get; set; }
    }
}

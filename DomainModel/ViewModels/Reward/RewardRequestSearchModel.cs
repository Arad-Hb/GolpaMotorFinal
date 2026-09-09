using Framework.Common;

namespace DomainModel.ViewModels.Reward
{
    public class RewardRequestSearchModel : PageModel
    {
        public string? SearchTerm { get; set; }
        public int? RewardDeliveryStatusID { get; set; }
        public bool? IsComplete { get; set; }

        public int? RewardCatalogID { get; set; }

        public DateTime? RequestFrom { get; set; }

        public DateTime? RequestTo { get; set; }

        public string? RequestFromJalali { get; set; }

        public string? RequestToJalali { get; set; }
    }
}

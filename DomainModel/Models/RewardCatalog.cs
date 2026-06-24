using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class RewardCatalog
    {
        public int RewardCatalogID { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int RequiredPoints { get; set; }

        public bool IsCashReward { get; set; }

        public decimal? CashValue { get; set; }

        public virtual ICollection<RewardRequest> RewardRequests { get; set; }
            = new HashSet<RewardRequest>();
    }
}

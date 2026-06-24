using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class RewardRequest
    {
        public int RewardRequestID { get; set; }

        public string UserID { get; set; } = null!;

        public int RewardCatalogID { get; set; }

        public DateTime? RequestDate { get; set; }

        public bool IsComplete { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual RewardCatalog RewardCatalog { get; set; } = null!;

        public virtual ICollection<PointTransaction> PointTransactions { get; set; }
            = new HashSet<PointTransaction>();
    }
}

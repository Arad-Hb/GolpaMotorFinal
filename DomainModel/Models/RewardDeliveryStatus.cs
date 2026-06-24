using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class RewardDeliveryStatus
    {
        public int RewardDeliveryStatusID { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public virtual ICollection<PointTransaction> PointTransactions { get; set; }
            = new HashSet<PointTransaction>();
    }
}

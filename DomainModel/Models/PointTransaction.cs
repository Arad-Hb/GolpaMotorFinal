using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class PointTransaction
    {
        public int PointTransactionID { get; set; }

        public DateTime? PointTransactionDate { get; set; }

        public int PointsAmount { get; set; }

        public int PointsBeforeTransaction { get; set; }

        public int PointsAfterTransaction { get; set; }

        public string? Description { get; set; }

        public int? RewardDeliveryStatusID { get; set; }

        public string? ReferencePostNumber { get; set; }

        public int? RewardRequestID { get; set; }

        public long? PaymentTransactionID { get; set; }

        public string UserID { get; set; } = null!;

        public virtual RewardDeliveryStatus RewardDeliveryStatus { get; set; } = null!;

        public virtual RewardRequest RewardRequest { get; set; } = null!;

        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }
}

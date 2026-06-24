using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class PaymentTransaction
    {
        public long PaymentTransactionID { get; set; }

        public DateTime PaymentDate { get; set; }

        public int TransactionTypeID { get; set; }

        public string ReferenceNumber { get; set; } = null!;

        public string? FishImage { get; set; }

        public string? OriginNumber { get; set; }

        public string? DestinationNumber { get; set; }

        public string? OriginBank { get; set; }

        public string? DestinationBank { get; set; }

        public string RegisteredByUserId { get; set; } = null!;

        public decimal PaymentAmount { get; set; }

        public decimal BalanceBeforeTransaction { get; set; }

        public decimal BalanceAfterTransaction { get; set; }

        public string? Description { get; set; }

        public virtual TransactionType TransactionType { get; set; } = null!;

        public virtual ICollection<PointTransaction> PointTransactions { get; set; }
            = new HashSet<PointTransaction>();
    }
}

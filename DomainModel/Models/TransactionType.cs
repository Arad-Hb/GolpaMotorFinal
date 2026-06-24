using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class TransactionType
    {
        public int TransactionTypeID { get; set; }

        public string TransactionTypeName { get; set; } = null!;

        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
            = new HashSet<PaymentTransaction>();
    }
}

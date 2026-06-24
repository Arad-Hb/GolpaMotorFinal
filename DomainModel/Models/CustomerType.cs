using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class CustomerType
    {
        public int CustomerTypeID { get; set; }

        public string Title { get; set; } = null!;

        public virtual ICollection<UserCustomerType> UserCustomerTypes { get; set; }
            = new HashSet<UserCustomerType>();
    }
}

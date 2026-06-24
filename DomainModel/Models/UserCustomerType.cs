using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class UserCustomerType
    {
        public string UserID { get; set; } = null!;

        public int CustomerTypeID { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual CustomerType CustomerType { get; set; } = null!;
    }
}

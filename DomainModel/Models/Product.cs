using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class Product
    {
        public long ProductID { get; set; }

        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; }

        public int ProductPoint { get; set; }

        public bool IsAvailable { get; set; }

        //public DateTime CreatedDate { get; set; } = DateTime.Now; 
        public virtual ICollection<WarrantyCard> WarrantyCards { get; set; }
            = new HashSet<WarrantyCard>();
    }
}

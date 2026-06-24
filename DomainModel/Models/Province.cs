using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class Province
    {
        public int ProvinceID { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<City> Cities { get; set; }
            = new HashSet<City>();

        public virtual ICollection<ApplicationUser> Users { get; set; }
            = new HashSet<ApplicationUser>();
    }
}

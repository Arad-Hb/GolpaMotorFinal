using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class City
    {
        public int CityID { get; set; }

        public string Name { get; set; } = null!;

        public int ProvinceID { get; set; }

        public virtual Province Province { get; set; } = null!;

        public virtual ICollection<ApplicationUser> Users { get; set; }
            = new HashSet<ApplicationUser>();
    }
}

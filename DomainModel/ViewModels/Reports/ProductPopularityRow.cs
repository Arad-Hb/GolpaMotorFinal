using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Reports
{
    public class ProductPopularityRow
    {
        public string ProductName { get; set; } = string.Empty;
        public int JalaliYear { get; set; }
        public int JalaliMonth { get; set; }
        public int Count { get; set; }
    }
}

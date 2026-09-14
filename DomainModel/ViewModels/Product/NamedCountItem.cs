using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Product
{
    public class NamedCountItem
    {
        [Display(Name = "نام")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "تعداد")]
        public int Count { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolpaMotorFinal.Models.ViewModels.ProductManagement
{
    public class ProductImportViewModel
    {
        public int ProductID { get; set; }

        public IFormFile ExcelFile { get; set; }

        public List<SelectListItem> Products { get; set; }
    }
}

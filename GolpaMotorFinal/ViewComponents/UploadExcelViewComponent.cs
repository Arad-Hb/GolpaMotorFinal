using DataAccess.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GolpaMotorFinal.ViewComponents
{
    [ViewComponent(Name = "UploadExcel")]
    public class UploadExcelViewComponent : ViewComponent
    {
        private readonly IProductRepository prodRepo;
        public UploadExcelViewComponent(IProductRepository prodRepo)
        {
            this.prodRepo = prodRepo;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await prodRepo.GetAll();
            return View(products);
        }
    }
}

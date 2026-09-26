using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    public class LookupController : Controller
    {
        private readonly ILookupService lookups;

        public LookupController(ILookupService lookups)
        {
            this.lookups = lookups;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> Cities(int provinceId)
        {
            var cities = await lookups.GetCitiesByProvinceId(provinceId);
            if (cities == null || cities.Count == 0)
                return Json(new { success = false, data = Array.Empty<object>(), message = "شهری یافت نشد" });

            return Json(new
            {
                success = true,
                data = cities.Select(c => new { cityID = c.CityID, name = c.Name })
            });
        }
    }
}

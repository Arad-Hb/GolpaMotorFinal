using Application.Services;
using DomainModel.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolpaMotorFinal.Helpers
{
    public class LookupLists
    {
        private readonly ILookupService lookups;

        public LookupLists(ILookupService lookups)
        {
            this.lookups = lookups;
        }

        public Task<List<Province>> Provinces() => lookups.GetProvinces();

        public Task<List<CustomerType>> CustomerTypes() => lookups.GetCustomerTypes();

        public Task<List<City>> Cities(int provinceId) => lookups.GetCitiesByProvinceId(provinceId);

        public async Task<IEnumerable<SelectListItem>> ProvinceItems(int? selected = null)
        {
            var items = await lookups.GetProvinces();
            return new SelectList(items, "ProvinceID", "Name", selected);
        }

        public async Task<IEnumerable<SelectListItem>> CityItems(int provinceId, int? selected = null)
        {
            var items = await lookups.GetCitiesByProvinceId(provinceId);
            return new SelectList(items, "CityID", "Name", selected);
        }

        public async Task<IEnumerable<SelectListItem>> CustomerTypeItems(int? selected = null)
        {
            var items = await lookups.GetCustomerTypes();
            return new SelectList(items, "CustomerTypeID", "Title", selected);
        }
    }
}

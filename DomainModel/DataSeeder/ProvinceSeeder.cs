using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class ProvinceSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.Provinces.Any())
                return;

            var provinces = new List<Province>
        {
            new() { Name = "تهران" },
            new() { Name = "اصفهان" },
            new() { Name = "خراسان رضوی" },
            new() { Name = "فارس" },
            new() { Name = "آذربایجان شرقی" },
            new() { Name = "گیلان" },
            new() { Name = "مازندران" },
            new() { Name = "زنجان" },
            new() { Name = "قزوین" },
            new() { Name = "البرز" }
        };

            await context.Provinces.AddRangeAsync(provinces);
            await context.SaveChangesAsync();
        }
    }
}

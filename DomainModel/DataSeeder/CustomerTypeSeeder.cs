using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class CustomerTypeSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.CustomerTypes.Any())
                return;

            var items = new List<CustomerType>
        {
            new() { Title = "نمایندگی" },
            new() { Title = "خرده فروش" },
            new() { Title = "فروشنده لوازم یدکی" },
            new() { Title = "مکانیک" },
            new() { Title = "تراشکار" },
            new() { Title = "جلوبندی ساز" },
            new() { Title = "باطری ساز" },
            new() { Title = "مصرف کننده نهایی" },
            new() { Title = "سایر" }
        };

            await context.CustomerTypes.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }
}

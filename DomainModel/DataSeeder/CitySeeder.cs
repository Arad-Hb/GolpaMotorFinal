using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class CitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.Cities.Any())
                return;

            var cities = new List<City>
        {
            new() { Name="تهران", ProvinceID=1 },
            new() { Name="اسلامشهر", ProvinceID=1 },
            new() { Name="شهریار", ProvinceID=1 },
            new() { Name="ورامین", ProvinceID=1 },
            new() { Name="پردیس", ProvinceID=1 },
            new() { Name="ری", ProvinceID=1 },
            new() { Name="رباط کریم", ProvinceID=1 },

            new() { Name="اصفهان", ProvinceID=2 },
            new() { Name="کاشان", ProvinceID=2 },
            new() { Name="نجف آباد", ProvinceID=2 },

            new() { Name="مشهد", ProvinceID=3 },
            new() { Name="نیشابور", ProvinceID=3 },
            new() { Name="سبزوار", ProvinceID=3 },

            new() { Name="شیراز", ProvinceID=4 },
            new() { Name="مرودشت", ProvinceID=4 },
            new() { Name="جهرم", ProvinceID=4 },

            new() { Name="تبریز", ProvinceID=5 },
            new() { Name="مراغه", ProvinceID=5 },
            new() { Name="میانه", ProvinceID=5 },

            new() { Name="رشت", ProvinceID=6 },
            new() { Name="لاهیجان", ProvinceID=6 },
            new() { Name="بندر انزلی", ProvinceID=6 },

            new() { Name="ساری", ProvinceID=7 },
            new() { Name="آمل", ProvinceID=7 },
            new() { Name="بابل", ProvinceID=7 },

            new() { Name="زنجان", ProvinceID=8 },
            new() { Name="ابهر", ProvinceID=8 },
            new() { Name="خرمدره", ProvinceID=8 },

            new() { Name="قزوین", ProvinceID=9 },
            new() { Name="الوند", ProvinceID=9 },
            new() { Name="تاکستان", ProvinceID=9 },

            new() { Name="کرج", ProvinceID=10 },
            new() { Name="فردیس", ProvinceID=10 },
            new() { Name="نظرآباد", ProvinceID=10 }
        };

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();
        }
    }
}

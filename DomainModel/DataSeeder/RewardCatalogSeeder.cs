using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class RewardCatalogSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.RewardCatalogs.Any())
                return;

            var items = new List<RewardCatalog>
        {
            new() { Title="لباس کار", Description="لباس کار صنعتی", RequiredPoints=500, IsCashReward=false },
            new() { Title="جعبه ابزار", Description="جعبه ابزار 24 پارچه", RequiredPoints=800, IsCashReward=false },
            new() { Title="آچار بکس", Description="ست آچار بکس حرفه‌ای", RequiredPoints=1200, IsCashReward=false },
            new() { Title="کارت هدیه 500 هزار تومانی", Description="واریز نقدی 500 هزار تومان", RequiredPoints=1000, IsCashReward=true, CashValue=500000 },
            new() { Title="کارت هدیه 1 میلیون تومانی", Description="واریز نقدی 1 میلیون تومان", RequiredPoints=1800, IsCashReward=true, CashValue=1000000 },
            new() { Title="کارت هدیه 2 میلیون تومانی", Description="واریز نقدی 2 میلیون تومان", RequiredPoints=3500, IsCashReward=true, CashValue=2000000 },
            new() { Title="دریل شارژی", Description="دریل شارژی صنعتی", RequiredPoints=2500, IsCashReward=false },
            new() { Title="مولتی متر", Description="مولتی متر دیجیتال", RequiredPoints=1500, IsCashReward=false },
            new() { Title="بن خرید 200 هزار تومانی", Description="جایزه نقدی", RequiredPoints=400, IsCashReward=true, CashValue=200000 },
            new() { Title="بن خرید 3 میلیون تومانی", Description="جایزه نقدی", RequiredPoints=5000, IsCashReward=true, CashValue=3000000 }
        };

            await context.RewardCatalogs.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }
}

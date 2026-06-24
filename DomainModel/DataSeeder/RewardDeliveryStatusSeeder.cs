using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class RewardDeliveryStatusSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.RewardDeliveryStatuses.Any())
                return;

            var items = new List<RewardDeliveryStatus>
        {
            new() { Title = "در انتظار بررسی" },
            new() { Title = "تایید شده" },
            new() { Title = "رد شده" },
            new() { Title = "پرداخت شده" },
            new() { Title = "لغو شده" }
        };

            await context.RewardDeliveryStatuses.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }
}

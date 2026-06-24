using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class TransactionTypeSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.TransactionTypes.Any())
                return;

            var items = new List<TransactionType>
        {
            new() { TransactionTypeName = "ثبت کارت گارانتی" },
            new() { TransactionTypeName = "پاداش ویژه" },
            new() { TransactionTypeName = "اصلاح امتیاز" },
            new() { TransactionTypeName = "کسر بابت دریافت جایزه" }
        };

            await context.TransactionTypes.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }
}

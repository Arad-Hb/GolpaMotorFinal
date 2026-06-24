using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class WarrantyCardSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.WarrantyCards.Any())
                return;

            var cards = new List<WarrantyCard>
        {
            new()
            {
                ProductID = 1,
                SerialNumber = "100000000001",
                ScratchedCode = "500001",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی سرسیلندر"
            },
            new()
            {
                ProductID = 1,
                SerialNumber = "100000000002",
                ScratchedCode = "500002",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی سرسیلندر"
            },

            new()
            {
                ProductID = 2,
                SerialNumber = "100000000004",
                ScratchedCode = "500004",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی میل لنگ"
            },
            new()
            {
                ProductID = 2,
                SerialNumber = "100000000005",
                ScratchedCode = "500005",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی میل لنگ"
            },

            new()
            {
                ProductID = 3,
                SerialNumber = "100000000007",
                ScratchedCode = "500007",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی رینگ موتور"
            },
            new()
            {
                ProductID = 3,
                SerialNumber = "100000000008",
                ScratchedCode = "500008",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی رینگ موتور"
            },

            new()
            {
                ProductID = 4,
                SerialNumber = "100000000010",
                ScratchedCode = "500010",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی پیستون"
            },
            new()
            {
                ProductID = 4,
                SerialNumber = "100000000011",
                ScratchedCode = "500011",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی پیستون"
            },

            new()
            {
                ProductID = 5,
                SerialNumber = "100000000013",
                ScratchedCode = "500013",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی یاتاقان"
            },
            new()
            {
                ProductID = 5,
                SerialNumber = "100000000015",
                ScratchedCode = "500015",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی یاتاقان"
            },

            new()
            {
                ProductID = 6,
                SerialNumber = "100000000016",
                ScratchedCode = "500016",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی سوپاپ"
            },
            new()
            {
                ProductID = 6,
                SerialNumber = "100000000018",
                ScratchedCode = "500018",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی سوپاپ"
            },

            new()
            {
                ProductID = 7,
                SerialNumber = "100000000019",
                ScratchedCode = "500019",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی شاتون"
            },
            new()
            {
                ProductID = 7,
                SerialNumber = "100000000020",
                ScratchedCode = "500020",
                ValidityMonths = 12,
                IsRegistered = false,
                Description = "گارانتی شاتون"
            }
        };

            await context.WarrantyCards.AddRangeAsync(cards);
            await context.SaveChangesAsync();
        }
    }
}

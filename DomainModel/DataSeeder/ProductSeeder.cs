using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.DataSeeder
{
    public static class ProductSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GolpaMotorDbContext>();

            if (context.Products.Any())
                return;

            var items = new List<Product>
        {
            new() { ProductName="سرسیلندر", Description="سرسیلندر موتور", ImageUrl="1.jpg", IsDeleted=false, ProductPoint=250, IsAvailable=true },
            new() { ProductName="میل لنگ", Description="میل لنگ موتور", ImageUrl="2.jpg", IsDeleted=false, ProductPoint=400, IsAvailable=true },
            new() { ProductName="رینگ پیستون", Description="رینگ پیستون", ImageUrl="3.jpg", IsDeleted=false, ProductPoint=150, IsAvailable=true },
            new() { ProductName="پیستون", Description="پیستون موتور", ImageUrl="4.jpg", IsDeleted=false, ProductPoint=300, IsAvailable=true },
            new() { ProductName="یاتاقان", Description="یاتاقان موتور", ImageUrl="5.jpg", IsDeleted=false, ProductPoint=120, IsAvailable=true },
            new() { ProductName="شاتون", Description="شاتون موتور", ImageUrl="6.jpg", IsDeleted=false, ProductPoint=350, IsAvailable=true },
            new() { ProductName="سوپاپ", Description="سوپاپ موتور", ImageUrl="7.jpg", IsDeleted=false, ProductPoint=180, IsAvailable=true },
            new() { ProductName="اویل پمپ", Description="پمپ روغن موتور", ImageUrl="8.jpg", IsDeleted=false, ProductPoint=220, IsAvailable=true },
            new() { ProductName="واشر سرسیلندر", Description="واشر موتور", ImageUrl="9.jpg", IsDeleted=false, ProductPoint=100, IsAvailable=true },
            new() { ProductName="بوش سیلندر", Description="بوش سیلندر موتور", ImageUrl="10.jpg", IsDeleted=false, ProductPoint=200, IsAvailable=true }
        };

            await context.Products.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }
}

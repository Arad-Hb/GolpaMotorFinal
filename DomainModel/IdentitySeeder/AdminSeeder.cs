using DomainModel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.IdentitySeeder
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            const string email = "aradhabashi@gmail.com";
            const string password = "aradhb369852";

            // آیا ادمین قبلاً ساخته شده؟
            var adminUser = await userManager.FindByEmailAsync(email);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,

                    FirstName = "آراد",
                    LastName = "حبشی",

                    IsActive = true,
                    IsConfirmedCode = true,

                    RegisterDate = DateTime.Now,

                    TotalEarnedPoints = 0,
                    TotalSettledPoints = 0,
                    RemainedPoints = 0,
                    TotalRegisteredCards = 0
                };

                var result = await userManager.CreateAsync(
                    adminUser,
                    password);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(" | ",
                        result.Errors.Select(x => x.Description)));
                }
            }

            // اگر در رول Admin نیست، اضافه کن
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

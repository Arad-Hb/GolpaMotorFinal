using DomainModel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainModel.IdentitySeeder
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration =
                serviceProvider.GetRequiredService<IConfiguration>();

            const string email = "aradhabashi@gmail.com";
            var password = configuration["AdminSeeder:Password"];

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("AdminSeeder:Password is not configured. Set it in User Secrets.");

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

                var result = await userManager.CreateAsync(adminUser, password);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(" | ",
                        result.Errors.Select(x => x.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

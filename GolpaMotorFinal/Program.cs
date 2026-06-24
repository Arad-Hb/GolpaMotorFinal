
using DataAccess.Repositories;
using DataAccess.Services;
using DomainModel.DataSeeder;
using DomainModel.IdentitySeeder;
using DomainModel.Models;
using GolpaMotorFinal.FrameworkUI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;



var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<GolpaMotorDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<GolpaMotorDbContext>()
.AddDefaultTokenProviders(); 

builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

//اکسل
ExcelPackage.License.SetNonCommercialOrganization("GolpaMotorFinal");

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICardRegistrationRepository, CardRegistrationRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileManager, FileManager>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});



var app = builder.Build();

//IdentitySeeder_DataSeeder
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    await RoleSeeder.SeedAsync(services);
//    await AdminSeeder.SeedAsync(services);

//    await ProvinceSeeder.SeedAsync(services);
//    await CitySeeder.SeedAsync(services);
//    await CustomerTypeSeeder.SeedAsync(services);
//    await TransactionTypeSeeder.SeedAsync(services);
//    await RewardCatalogSeeder.SeedAsync(services);
//    await ProductSeeder.SeedAsync(services);
//    await WarrantyCardSeeder.SeedAsync(services);
//    await RewardDeliveryStatusSeeder.SeedAsync(services);
//}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

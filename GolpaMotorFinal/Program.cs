
using DataAccess.Repositories;
using DataAccess.Services;
using DomainModel.DataSeeder;
using DomainModel.Models;
using GolpaMotorFinal.FrameworkUI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<GolpaMotorDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

ExcelPackage.License.SetNonCommercialOrganization("GolpaMotorFinal");

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICardRegistrationRepository, CardRegistrationRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarrantyCardRepository, WarrantyCardRepository>();
builder.Services.AddScoped<IRewardCatalogRepository, RewardCatalogRepository>();
builder.Services.AddScoped<IRewardRequestRepository, RewardRequestRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileManager, FileManager>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWarrantyExcelService, WarrantyExcelService>();
builder.Services.AddScoped<IGridConfigurationFactory, GridConfigurationFactory>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

using (var scope = app.Services.CreateScope())
{
    await RewardDeliveryStatusSeeder.SeedAsync(scope.ServiceProvider);
    await RewardCatalogSeeder.SeedAsync(scope.ServiceProvider);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

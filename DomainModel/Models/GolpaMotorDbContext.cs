using DomainModel.Models.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class GolpaMotorDbContext : IdentityDbContext<ApplicationUser>
    {
        public GolpaMotorDbContext(DbContextOptions<GolpaMotorDbContext> options) : base(options)
        {

        }
        public DbSet<CardRegistration> CardRegistrations { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<RewardCatalog> RewardCatalogs { get; set; }
        public DbSet<RewardDeliveryStatus> RewardDeliveryStatuses { get; set; }
        public DbSet<RewardRequest> RewardRequests { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<UserCustomerType> UserCustomerTypes { get; set; }
        public DbSet<WarrantyCard> WarrantyCards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());

            modelBuilder.ApplyConfiguration(new CardRegistrationConfiguration());

            modelBuilder.ApplyConfiguration(new CityConfiguration());

            modelBuilder.ApplyConfiguration(new CustomerTypeConfiguration());

            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());

            modelBuilder.ApplyConfiguration(new PointTransactionConfiguration());

            modelBuilder.ApplyConfiguration(new ProductConfiguration());

            modelBuilder.ApplyConfiguration(new ProvinceConfiguration());

            modelBuilder.ApplyConfiguration(new RewardCatalogConfiguration());

            modelBuilder.ApplyConfiguration(new RewardDeliveryStatusConfiguration());

            modelBuilder.ApplyConfiguration(new RewardRequestConfiguration());

            modelBuilder.ApplyConfiguration(new TransactionTypeConfiguration());

            modelBuilder.ApplyConfiguration(new UserCustomerTypeConfiguration());

            modelBuilder.ApplyConfiguration(new WarrantyCardConfiguration());
        }
    }
}

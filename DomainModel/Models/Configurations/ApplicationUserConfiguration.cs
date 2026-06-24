using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.FirstName)
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .HasMaxLength(200);

            builder.Property(x => x.CreditCartNumber)
                .HasMaxLength(20);

            builder.Property(x => x.IBAN)
                .HasMaxLength(50);

            builder.Property(x => x.AccountNumber)
                .HasMaxLength(50);

            builder.Property(x => x.VerificationCodeHash)
                .HasMaxLength(50);

            builder.Property(x => x.VerificationCodeSalt)
                .HasMaxLength(50);

            builder.Property(x => x.VerificationCodePurpose)
                .HasMaxLength(50);

            builder.Property(x => x.VerificationCodeDestination)
                .HasMaxLength(50);

            builder.HasOne(x => x.Province)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.ProvinceID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.City)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.CityID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

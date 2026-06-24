using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class CardRegistrationConfiguration : IEntityTypeConfiguration<CardRegistration>
    {
        public void Configure(EntityTypeBuilder<CardRegistration> builder)
        {
            builder.HasKey(x => x.CardRegisterationID);

            builder.Property(x => x.SerialNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ScratchedCode)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CustomerPhoneNumber)
                .HasMaxLength(13)
                .IsRequired();

            builder.Property(x => x.FaildMessage)
                .HasMaxLength(500);

            builder.Property(x => x.SuccessMessage)
                .HasMaxLength(500);

            builder.HasOne(x => x.User)
                .WithMany(x => x.CardRegistrations)
                .HasForeignKey(x => x.UserID);

            builder.HasOne(x => x.WarrantyCard)
                .WithMany(x => x.CardRegistrations)
                .HasForeignKey(x => x.WarrantyCardID);
        }
    }
}

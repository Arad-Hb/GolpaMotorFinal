using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class WarrantyCardConfiguration : IEntityTypeConfiguration<WarrantyCard>
    {
        public void Configure(EntityTypeBuilder<WarrantyCard> builder)
        {
            builder.HasKey(x => x.WarrantyCardID);

            builder.Property(x => x.SerialNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ScratchedCode)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.WarrantyCards)
                .HasForeignKey(x => x.ProductID);
        }
    }
}

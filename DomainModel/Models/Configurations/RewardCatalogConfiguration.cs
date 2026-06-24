using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class RewardCatalogConfiguration : IEntityTypeConfiguration<RewardCatalog>
    {
        public void Configure(EntityTypeBuilder<RewardCatalog> builder)
        {
            builder.HasKey(x => x.RewardCatalogID);

            builder.Property(x => x.Title)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(400);

            builder.Property(x => x.CashValue)
                .HasPrecision(18, 2);
        }
    }
}

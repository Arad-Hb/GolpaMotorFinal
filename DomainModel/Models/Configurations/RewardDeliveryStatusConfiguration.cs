using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class RewardDeliveryStatusConfiguration : IEntityTypeConfiguration<RewardDeliveryStatus>
    {
        public void Configure(EntityTypeBuilder<RewardDeliveryStatus> builder)
        {
            builder.HasKey(x => x.RewardDeliveryStatusID);

            builder.Property(x => x.Title)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(400);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
    {
        public void Configure(EntityTypeBuilder<PointTransaction> builder)
        {
            builder.HasKey(x => x.PointTransactionID);

            builder.Property(x => x.Description)
                .HasMaxLength(200);

            builder.Property(x => x.ReferencePostNumber)
                .HasMaxLength(50);

            builder.HasOne(x => x.RewardDeliveryStatus)
                .WithMany(x => x.PointTransactions)
                .HasForeignKey(x => x.RewardDeliveryStatusID);

            builder.HasOne(x => x.RewardRequest)
                .WithMany(x => x.PointTransactions)
                .HasForeignKey(x => x.RewardRequestID);

            builder.HasOne(x => x.PaymentTransaction)
                .WithMany(x => x.PointTransactions)
                .HasForeignKey(x => x.PaymentTransactionID);
        }
    }
}

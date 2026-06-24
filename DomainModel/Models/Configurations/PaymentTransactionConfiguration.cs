using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.HasKey(x => x.PaymentTransactionID);

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FishImage)
                .HasMaxLength(500);

            builder.Property(x => x.OriginNumber)
                .HasMaxLength(100);

            builder.Property(x => x.DestinationNumber)
                .HasMaxLength(100);

            builder.Property(x => x.OriginBank)
                .HasMaxLength(100);

            builder.Property(x => x.DestinationBank)
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.PaymentAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.BalanceBeforeTransaction)
                .HasPrecision(18, 2);

            builder.Property(x => x.BalanceAfterTransaction)
                .HasPrecision(18, 2);

            builder.HasOne(x => x.TransactionType)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.TransactionTypeID);
        }
    }
}

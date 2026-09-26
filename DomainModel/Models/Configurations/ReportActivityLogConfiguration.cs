using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainModel.Models.Configurations
{
    public class ReportActivityLogConfiguration : IEntityTypeConfiguration<ReportActivityLog>
    {
        public void Configure(EntityTypeBuilder<ReportActivityLog> builder)
        {
            builder.HasKey(x => x.ReportActivityLogID);

            builder.Property(x => x.SourceKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ActivityType).HasMaxLength(40).IsRequired();
            builder.Property(x => x.UserID).HasMaxLength(450).IsRequired();
            builder.Property(x => x.StatusTitle).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.SourceKey).IsUnique();
            builder.HasIndex(x => new { x.ProductID, x.OccurredAtUtc });
            builder.HasIndex(x => new { x.UserID, x.OccurredAtUtc });
            builder.HasIndex(x => new { x.WarrantyCardID, x.OccurredAtUtc });
            builder.HasIndex(x => new { x.RewardRequestID, x.OccurredAtUtc });
            builder.HasIndex(x => new { x.ActivityType, x.OccurredAtUtc });

            builder.HasOne(x => x.User)
                .WithMany(x => x.ReportActivityLogs)
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ReportActivityLogs)
                .HasForeignKey(x => x.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WarrantyCard)
                .WithMany(x => x.ReportActivityLogs)
                .HasForeignKey(x => x.WarrantyCardID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CardRegistration)
                .WithMany(x => x.ReportActivityLogs)
                .HasForeignKey(x => x.CardRegistrationID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RewardRequest)
                .WithMany(x => x.ReportActivityLogs)
                .HasForeignKey(x => x.RewardRequestID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PointTransaction)
                .WithMany(x => x.ReportActivityLogs)
                .HasForeignKey(x => x.PointTransactionID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

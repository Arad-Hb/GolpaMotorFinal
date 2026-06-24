using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class RewardRequestConfiguration : IEntityTypeConfiguration<RewardRequest>
    {
        public void Configure(EntityTypeBuilder<RewardRequest> builder)
        {
            builder.HasKey(x => x.RewardRequestID);

            builder.HasOne(x => x.User)
                .WithMany(x => x.RewardRequests)
                .HasForeignKey(x => x.UserID);

            builder.HasOne(x => x.RewardCatalog)
                .WithMany(x => x.RewardRequests)
                .HasForeignKey(x => x.RewardCatalogID);
        }
    }
}

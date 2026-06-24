using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class UserCustomerTypeConfiguration : IEntityTypeConfiguration<UserCustomerType>
    {
        public void Configure(EntityTypeBuilder<UserCustomerType> builder)
        {
            builder.HasKey(x => new
            {
                x.UserID,
                x.CustomerTypeID
            });

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserCustomerTypes)
                .HasForeignKey(x => x.UserID);

            builder.HasOne(x => x.CustomerType)
                .WithMany(x => x.UserCustomerTypes)
                .HasForeignKey(x => x.CustomerTypeID);
        }
    }
}

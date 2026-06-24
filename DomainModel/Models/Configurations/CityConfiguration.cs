using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasKey(x => x.CityID);

            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(x => x.Province)
                .WithMany(x => x.Cities)
                .HasForeignKey(x => x.ProvinceID);
        }
    }
}

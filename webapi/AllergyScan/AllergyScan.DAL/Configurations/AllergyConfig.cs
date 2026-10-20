using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllergyScan.DAL.Configurations
{
    public class AllergyConfig : IEntityTypeConfiguration<Allergy>
    {
        public void Configure(EntityTypeBuilder<Allergy> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100)
                .UseCollation("NOCASE");

            builder.HasIndex(a => a.Name).IsUnique();
        }
    }
}

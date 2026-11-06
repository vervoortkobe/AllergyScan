using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AllergyScan.DAL.Model;

namespace AllergyScan.DAL.Configurations
{
    public class AllergyConfig : IEntityTypeConfiguration<Allergy>
    {
        public void Configure(EntityTypeBuilder<Allergy> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                .IsRequired()
                .HasDefaultValue(1); // Single-user app: always user 1

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100)
                .UseCollation("NOCASE");

            builder.Property(a => a.Flag)
                .IsRequired()
                .HasDefaultValue(ScanFlag.Contains)
                .HasSentinel(ScanFlag.Contains);

            builder.HasIndex(a => a.UserId);
            // Allergy names are unique per user. With the implicit single-user
            // model every row has UserId = 1, so this behaves like a global
            // unique name while staying ready for multi-user support.
            builder.HasIndex(a => new { a.UserId, a.Name }).IsUnique();
        }
    }
}

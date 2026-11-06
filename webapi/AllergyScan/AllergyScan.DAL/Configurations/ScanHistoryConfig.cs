using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AllergyScan.DAL.Model;

namespace AllergyScan.DAL.Configurations
{
    public class ScanHistoryConfig : IEntityTypeConfiguration<ScanHistory>
    {
        public void Configure(EntityTypeBuilder<ScanHistory> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                .IsRequired()
                .HasDefaultValue(1); // Single-user app: always user 1

            builder.Property(s => s.Text)
                .IsRequired();

            builder.Property(s => s.ImageContentType).HasMaxLength(100);
            builder.Property(s => s.ImageFileName).HasMaxLength(260);

            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.ScannedAt);
            builder.HasIndex(s => new { s.UserId, s.ScannedAt });
        }
    }
}

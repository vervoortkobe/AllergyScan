using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllergyScan.DAL.Configurations
{
    public class ScanHistoryConfig : IEntityTypeConfiguration<ScanHistory>
    {
        public void Configure(EntityTypeBuilder<ScanHistory> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Text)
                .IsRequired();

            builder.Property(s => s.ImageContentType).HasMaxLength(100);
            builder.Property(s => s.ImageFileName).HasMaxLength(260);

            builder.HasIndex(s => s.ScannedAt);
        }
    }
}

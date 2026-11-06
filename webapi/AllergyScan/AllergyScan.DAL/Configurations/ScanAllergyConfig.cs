using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AllergyScan.DAL.Model;

namespace AllergyScan.DAL.Configurations
{
    public class ScanAllergyConfig : IEntityTypeConfiguration<ScanAllergy>
    {
        public void Configure(EntityTypeBuilder<ScanAllergy> builder)
        {
            builder.HasKey(sa => new { sa.ScanId, sa.AllergyId });

            builder.Property(sa => sa.UserId)
                .IsRequired()
                .HasDefaultValue(1); // Single-user app: always user 1

            builder.HasOne(sa => sa.Scan)
                .WithMany(s => s.ScanAllergies)
                .HasForeignKey(sa => sa.ScanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sa => sa.Allergy)
                .WithMany(a => a.ScanAllergies)
                .HasForeignKey(sa => sa.AllergyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(sa => sa.ScanId);
            builder.HasIndex(sa => sa.AllergyId);
            builder.HasIndex(sa => sa.UserId);
            builder.HasIndex(sa => new { sa.ScanId, sa.AllergyId });
        }
    }
}
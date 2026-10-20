using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllergyScan.DAL.Configurations
{
    public class LanguageConfig : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(60);

            builder.HasIndex(l => l.Code).IsUnique();

            builder.HasData(
                new Language { Id = 1, Code = "en", Name = "English" },
                new Language { Id = 2, Code = "nl", Name = "Dutch" },
                new Language { Id = 3, Code = "fr", Name = "French" },
                new Language { Id = 4, Code = "de", Name = "German" },
                new Language { Id = 5, Code = "es", Name = "Spanish" });
        }
    }
}

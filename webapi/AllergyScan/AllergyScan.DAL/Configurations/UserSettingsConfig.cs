using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllergyScan.DAL.Configurations
{
    public class UserSettingsConfig : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).ValueGeneratedNever();

            builder.HasOne(s => s.Language)
                .WithMany(l => l.UserSettings)
                .HasForeignKey(s => s.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Single-user app: one row, defaulting to English (language id 1).
            builder.HasData(new UserSettings { Id = UserSettings.SingletonId, LanguageId = 1 });
        }
    }
}

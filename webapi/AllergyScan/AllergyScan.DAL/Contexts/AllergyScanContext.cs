using Microsoft.EntityFrameworkCore;

namespace AllergyScan.DAL.Contexts
{
    public class AllergyScanContext : DbContext
    {
        public DbSet<Allergy> Allergies => Set<Allergy>();

        public DbSet<Language> Languages => Set<Language>();

        public DbSet<UserSettings> UserSettings => Set<UserSettings>();

        public DbSet<ScanHistory> Scans => Set<ScanHistory>();

        public AllergyScanContext(DbContextOptions<AllergyScanContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AllergyScanContext).Assembly);
        }
    }
}

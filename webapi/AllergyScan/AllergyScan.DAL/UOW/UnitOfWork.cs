using AllergyScan.DAL.Contexts;
using AllergyScan.DAL.Repositories;

namespace AllergyScan.DAL.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AllergyScanContext _context;

        public UnitOfWork(
            AllergyScanContext context,
            IAllergyRepository allergies,
            ILanguageRepository languages,
            IUserSettingsRepository userSettings,
            IScanHistoryRepository scans)
        {
            _context = context;
            Allergies = allergies;
            Languages = languages;
            UserSettings = userSettings;
            Scans = scans;
        }

        public IAllergyRepository Allergies { get; }

        public ILanguageRepository Languages { get; }

        public IUserSettingsRepository UserSettings { get; }

        public IScanHistoryRepository Scans { get; }

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}

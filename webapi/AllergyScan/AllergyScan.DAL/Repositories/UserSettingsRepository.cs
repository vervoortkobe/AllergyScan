using AllergyScan.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AllergyScan.DAL.Repositories
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly AllergyScanContext _context;

        public UserSettingsRepository(AllergyScanContext context)
        {
            _context = context;
        }

        public Task<UserSettings?> GetAsync(CancellationToken cancellationToken = default)
        {
            return _context.UserSettings
                .Include(s => s.Language)
                .FirstOrDefaultAsync(s => s.Id == UserSettings.SingletonId, cancellationToken);
        }

        public void Create(UserSettings settings)
        {
            _context.UserSettings.Add(settings);
        }
    }
}

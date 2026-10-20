using AllergyScan.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AllergyScan.DAL.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly AllergyScanContext _context;

        public LanguageRepository(AllergyScanContext context)
        {
            _context = context;
        }

        public async Task<List<Language>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Languages
                .AsNoTracking()
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public Task<Language?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Languages.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return _context.Languages.FirstOrDefaultAsync(l => l.Code == code, cancellationToken);
        }
    }
}

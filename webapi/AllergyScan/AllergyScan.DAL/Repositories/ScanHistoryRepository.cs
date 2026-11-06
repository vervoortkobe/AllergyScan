using AllergyScan.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AllergyScan.DAL.Repositories
{
    public class ScanHistoryRepository : IScanHistoryRepository
    {
        private readonly AllergyScanContext _context;

        public ScanHistoryRepository(AllergyScanContext context)
        {
            _context = context;
        }

        public async Task<(List<ScanHistory> Items, int Total)> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Scans.AsNoTracking();
            var total = await query.CountAsync(cancellationToken);

            // Do not load the image BLOB for the history list, but do load the
            // many-to-many allergies so the app can show what was detected.
            var items = await query
                .OrderByDescending(s => s.ScannedAt)
                .ThenByDescending(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ScanHistory
                {
                    Id = s.Id,
                    Text = s.Text,
                    Flag = s.Flag,
                    ScannedAt = s.ScannedAt,
                    ImageContentType = s.ImageContentType,
                    ImageFileName = s.ImageFileName,
                    Image = null,
                    ScanAllergies = s.ScanAllergies
                        .Select(sa => new ScanAllergy
                        {
                            ScanId = sa.ScanId,
                            AllergyId = sa.AllergyId,
                            DetectionFlag = sa.DetectionFlag,
                            Notes = sa.Notes,
                            Allergy = new Allergy
                            {
                                Id = sa.Allergy.Id,
                                Name = sa.Allergy.Name,
                                Flag = sa.Allergy.Flag
                            }
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public Task<ScanHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Scans
                .Include(s => s.ScanAllergies)
                    .ThenInclude(sa => sa.Allergy)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public void Create(ScanHistory scan)
        {
            _context.Scans.Add(scan);
        }

        public void Update(ScanHistory scan)
        {
            _context.Scans.Update(scan);
        }

        public void Delete(ScanHistory scan)
        {
            _context.Scans.Remove(scan);
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            // Remove the junction rows first so the operation also works when
            // SQLite foreign-key cascading is not enabled.
            await _context.ScanAllergies.ExecuteDeleteAsync(cancellationToken);
            await _context.Scans.ExecuteDeleteAsync(cancellationToken);
        }
    }
}

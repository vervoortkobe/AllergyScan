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

            // Do not load the image BLOB for the history list.
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
                    Image = null
                })
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public Task<ScanHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Scans.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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

        public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            return _context.Scans.ExecuteDeleteAsync(cancellationToken);
        }
    }
}

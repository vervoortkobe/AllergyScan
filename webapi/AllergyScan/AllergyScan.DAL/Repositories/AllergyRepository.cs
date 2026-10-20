using AllergyScan.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AllergyScan.DAL.Repositories
{
    public class AllergyRepository : IAllergyRepository
    {
        private readonly AllergyScanContext _context;

        public AllergyRepository(AllergyScanContext context)
        {
            _context = context;
        }

        public async Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Allergies
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync(cancellationToken);
        }

        public Task<Allergy?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Allergies.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public void Create(Allergy allergy)
        {
            _context.Allergies.Add(allergy);
        }

        public void Update(Allergy allergy)
        {
            _context.Allergies.Update(allergy);
        }

        public void Delete(Allergy allergy)
        {
            _context.Allergies.Remove(allergy);
        }
    }
}

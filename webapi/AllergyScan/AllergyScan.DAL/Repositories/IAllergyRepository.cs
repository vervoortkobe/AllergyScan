namespace AllergyScan.DAL.Repositories
{
    public interface IAllergyRepository
    {
        Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Allergy?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        void Create(Allergy allergy);

        void Update(Allergy allergy);

        void Delete(Allergy allergy);
    }
}

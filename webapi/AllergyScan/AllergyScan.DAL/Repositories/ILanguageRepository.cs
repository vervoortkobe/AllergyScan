namespace AllergyScan.DAL.Repositories
{
    public interface ILanguageRepository
    {
        Task<List<Language>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Language?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}

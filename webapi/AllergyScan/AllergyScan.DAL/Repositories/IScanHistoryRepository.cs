namespace AllergyScan.DAL.Repositories
{
    public interface IScanHistoryRepository
    {
        Task<(List<ScanHistory> Items, int Total)> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<ScanHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        void Create(ScanHistory scan);

        void Update(ScanHistory scan);

        void Delete(ScanHistory scan);

        Task DeleteAllAsync(CancellationToken cancellationToken = default);
    }
}

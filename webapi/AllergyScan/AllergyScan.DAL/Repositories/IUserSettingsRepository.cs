namespace AllergyScan.DAL.Repositories
{
    public interface IUserSettingsRepository
    {
        Task<UserSettings?> GetAsync(CancellationToken cancellationToken = default);

        void Create(UserSettings settings);
    }
}

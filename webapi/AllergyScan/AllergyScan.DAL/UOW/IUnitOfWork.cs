using AllergyScan.DAL.Repositories;

namespace AllergyScan.DAL.UOW
{
    public interface IUnitOfWork
    {
        IAllergyRepository Allergies { get; }

        ILanguageRepository Languages { get; }

        IUserSettingsRepository UserSettings { get; }

        IScanHistoryRepository Scans { get; }

        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}

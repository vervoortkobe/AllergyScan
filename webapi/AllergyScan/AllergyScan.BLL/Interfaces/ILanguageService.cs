using AllergyScan.BLL.DTO;

namespace AllergyScan.BLL.Interfaces
{
    public interface ILanguageService
    {
        Task<List<LanguageDTO>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns the selected language. There is always one (English by default).</summary>
        Task<LanguageDTO> GetCurrentAsync(CancellationToken cancellationToken = default);

        Task<LanguageDTO> SetCurrentAsync(SetLanguageRequest request, CancellationToken cancellationToken = default);
    }
}

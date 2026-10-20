using AllergyScan.BLL.DTO;
using AllergyScan.BLL.Exceptions;
using AllergyScan.BLL.Interfaces;
using AllergyScan.DAL;
using AllergyScan.DAL.UOW;

namespace AllergyScan.BLL.Services
{
    public class LanguageService : ILanguageService
    {
        private const string DefaultLanguageCode = "en";

        private readonly IUnitOfWork _unitOfWork;

        public LanguageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<LanguageDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var languages = await _unitOfWork.Languages.GetAllAsync(cancellationToken);
            return languages.Select(Map).ToList();
        }

        public async Task<LanguageDTO> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            var settings = await _unitOfWork.UserSettings.GetAsync(cancellationToken);

            if (settings?.Language is not null)
            {
                return Map(settings.Language);
            }

            // Defensive fallback: the database seed should always provide English.
            var fallback = await _unitOfWork.Languages.GetByCodeAsync(DefaultLanguageCode, cancellationToken)
                ?? (await _unitOfWork.Languages.GetAllAsync(cancellationToken)).FirstOrDefault()
                ?? throw new InvalidOperationException("No languages are configured in the database.");

            if (settings is null)
            {
                _unitOfWork.UserSettings.Create(new UserSettings
                {
                    Id = UserSettings.SingletonId,
                    LanguageId = fallback.Id
                });
            }
            else
            {
                settings.LanguageId = fallback.Id;
                settings.Language = fallback;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Map(fallback);
        }

        public async Task<LanguageDTO> SetCurrentAsync(SetLanguageRequest request, CancellationToken cancellationToken = default)
        {
            var code = (request.Code ?? string.Empty).Trim().ToLowerInvariant();

            if (code.Length == 0)
            {
                throw new BadRequestException("A language code is required.");
            }

            var language = await _unitOfWork.Languages.GetByCodeAsync(code, cancellationToken)
                ?? throw new BadRequestException($"Unsupported language code '{code}'. Use GET /api/languages to see the supported languages.");

            var settings = await _unitOfWork.UserSettings.GetAsync(cancellationToken);

            if (settings is null)
            {
                _unitOfWork.UserSettings.Create(new UserSettings
                {
                    Id = UserSettings.SingletonId,
                    LanguageId = language.Id
                });
            }
            else
            {
                settings.LanguageId = language.Id;
                settings.Language = language;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Map(language);
        }

        private static LanguageDTO Map(Language language)
        {
            return new LanguageDTO(language.Id, language.Code, language.Name);
        }
    }
}

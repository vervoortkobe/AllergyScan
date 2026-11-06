using AllergyScan.BLL.DTO;
using AllergyScan.BLL.Exceptions;
using AllergyScan.BLL.Interfaces;
using AllergyScan.DAL;
using AllergyScan.DAL.UOW;

namespace AllergyScan.BLL.Services
{
    public class AllergyService : IAllergyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AllergyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AllergyDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var allergies = await _unitOfWork.Allergies.GetAllAsync(cancellationToken);
            return allergies.Select(Map).ToList();
        }

        public async Task<AllergyDTO> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var allergy = await _unitOfWork.Allergies.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Allergy with id {id} was not found.");

            return Map(allergy);
        }

        public async Task<AllergyDTO> CreateAsync(CreateAllergyRequest request, CancellationToken cancellationToken = default)
        {
            EnsureFlagIsValid(request.Flag);

            var name = NormalizeName(request.Name);

            await EnsureNameIsUniqueAsync(name, null, cancellationToken);

            var allergy = new Allergy
            {
                Name = name,
                Flag = request.Flag,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Allergies.Create(allergy);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Map(allergy);
        }

        public async Task<AllergyDTO> UpdateAsync(int id, UpdateAllergyRequest request, CancellationToken cancellationToken = default)
        {
            var allergy = await _unitOfWork.Allergies.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Allergy with id {id} was not found.");

            var name = NormalizeName(request.Name);
            await EnsureNameIsUniqueAsync(name, id, cancellationToken);

            if (request.Flag is not null)
            {
                EnsureFlagIsValid(request.Flag.Value);
            }

            allergy.Name = name;

            if (request.Flag is not null)
            {
                allergy.Flag = request.Flag.Value;
            }

            _unitOfWork.Allergies.Update(allergy);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Map(allergy);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var allergy = await _unitOfWork.Allergies.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Allergy with id {id} was not found.");

            _unitOfWork.Allergies.Delete(allergy);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        private async Task EnsureNameIsUniqueAsync(string name, int? excludedId, CancellationToken cancellationToken)
        {
            var allergies = await _unitOfWork.Allergies.GetAllAsync(cancellationToken);

            if (allergies.Any(a => a.Id != excludedId && string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ConflictException($"An allergy named '{name}' already exists.");
            }
        }

        private static string NormalizeName(string? name)
        {
            var normalized = string.Join(' ', (name ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

            if (normalized.Length == 0)
            {
                throw new BadRequestException("The allergy name cannot be empty.");
            }

            return normalized;
        }

        private static void EnsureFlagIsValid(ScanFlag flag)
        {
            if (flag == ScanFlag.NoAllergen)
            {
                throw new BadRequestException(
                    "An allergy must have a severity of Contains, MayContain or MayContainTraces. Remove the allergy instead of setting it to NoAllergen.");
            }
        }

        private static AllergyDTO Map(Allergy allergy)
        {
            // SQLite stores DateTime as text and loses the Kind, so restore UTC for the API.
            var createdAt = DateTime.SpecifyKind(allergy.CreatedAt, DateTimeKind.Utc);
            return new AllergyDTO(allergy.Id, allergy.Name, createdAt, allergy.Flag);
        }
    }
}

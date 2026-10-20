using AllergyScan.BLL.DTO;

namespace AllergyScan.BLL.Interfaces
{
    public interface IAllergyService
    {
        Task<List<AllergyDTO>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<AllergyDTO> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<AllergyDTO> CreateAsync(CreateAllergyRequest request, CancellationToken cancellationToken = default);

        Task<AllergyDTO> UpdateAsync(int id, UpdateAllergyRequest request, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

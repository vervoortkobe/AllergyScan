using AllergyScan.BLL.DTO;

namespace AllergyScan.BLL.Interfaces
{
    public interface IScanService
    {
        Task<ScanHistoryDTO> CreateAsync(CreateScanRequest request, CancellationToken cancellationToken = default);

        Task<PagedResult<ScanHistoryDTO>> GetHistoryAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<ScanHistoryDTO> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<(byte[] Data, string ContentType, string FileName)> GetImageAsync(int id, CancellationToken cancellationToken = default);

        Task<ScanHistoryDTO> UpdateAsync(int id, UpdateScanRequest request, CancellationToken cancellationToken = default);

        Task<ScanHistoryDTO> UpdateImageAsync(int id, Stream image, string? contentType, string? fileName, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task DeleteAllAsync(CancellationToken cancellationToken = default);
    }
}

using AllergyScan.DAL;
using System.ComponentModel.DataAnnotations;

namespace AllergyScan.BLL.DTO
{
    public sealed record CreateScanRequest(
        Stream? Image,
        string? ImageContentType,
        string? ImageFileName,
        string? Text,
        ScanFlag Flag);

    public sealed class UpdateScanRequest
    {
        [StringLength(100_000)]
        public string? Text { get; set; }

        public ScanFlag? Flag { get; set; }
    }

    public sealed record ScanHistoryDTO(
        int Id,
        string Text,
        ScanFlag Flag,
        DateTime ScannedAt,
        bool HasImage,
        string? ImageUrl);

    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages);
}

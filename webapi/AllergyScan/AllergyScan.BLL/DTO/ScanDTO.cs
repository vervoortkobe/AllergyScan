using AllergyScan.DAL;
using System.ComponentModel.DataAnnotations;

namespace AllergyScan.BLL.DTO
{
    /// <summary>Request payload used to store a scan that was already processed by the app.</summary>
    public sealed record CreateScanRequest(
        Stream Image,
        string? ImageContentType,
        string? ImageFileName,
        string? Text,
        ScanFlag Flag,
        IReadOnlyList<int>? AllergyIds = null);

    public sealed class UpdateScanRequest
    {
        [StringLength(100_000)]
        public string? Text { get; set; }

        public ScanFlag? Flag { get; set; }

        /// <summary>
        /// Optional replacement of the detected allergies. When null the existing
        /// associations are left untouched; an empty list clears them.
        /// </summary>
        public IReadOnlyList<int>? AllergyIds { get; set; }
    }

    /// <summary>Request payload used to replace the allergies detected in a scan.</summary>
    public sealed class UpdateScanAllergiesRequest
    {
        [Required]
        public IReadOnlyList<int> AllergyIds { get; set; } = [];
    }

    /// <summary>One allergen that was detected in a scan (the junction row).</summary>
    public sealed record ScanAllergyDTO(
        int AllergyId,
        string Name,
        ScanFlag AllergyFlag,
        ScanFlag DetectionFlag,
        string? Notes);

    public sealed record ScanHistoryDTO(
        int Id,
        string Text,
        ScanFlag Flag,
        DateTime ScannedAt,
        bool HasImage,
        string? ImageUrl,
        IReadOnlyList<ScanAllergyDTO> Allergies);

    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages);
}

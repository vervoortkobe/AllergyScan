using System.ComponentModel.DataAnnotations;
using AllergyScan.DAL;

namespace AllergyScan.BLL.DTO
{
    public sealed class CreateAllergyRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        public ScanFlag Flag { get; set; } = ScanFlag.AllergenDetected;
    }

    public sealed class UpdateAllergyRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        public ScanFlag? Flag { get; set; }
    }

    public sealed record AllergyDTO(int Id, string Name, DateTime CreatedAt, ScanFlag Flag);
}

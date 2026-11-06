using AllergyScan.DAL;
using System.ComponentModel.DataAnnotations;

namespace AllergyScan.WebAPI.Models
{
    public sealed class CreateScanForm
    {
        /// <summary>Required photo of the product or ingredients list.</summary>
        [Required]
        public IFormFile Image { get; set; } = null!;

        /// <summary>Ingredient text produced by the app's OCR provider.</summary>
        [Required]
        [StringLength(100_000)]
        public string Text { get; set; } = string.Empty;

        /// <summary>Result of the app's allergen check.</summary>
        [Required]
        public ScanFlag? Flag { get; set; }

        /// <summary>
        /// Optional explicit list of allergy ids detected in this scan. When omitted
        /// the API derives the associations from the ingredient text.
        /// </summary>
        public int[]? AllergyIds { get; set; }
    }
}

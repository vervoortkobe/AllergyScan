using System.ComponentModel.DataAnnotations;
using AllergyScan.DAL;
using Microsoft.AspNetCore.Http;

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
    }
}

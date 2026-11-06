using System;

namespace AllergyScan.DAL.Model
{
    /// <summary>
    /// Junction table for many-to-many relationship between ScanHistory and Allergy.
    /// Represents individual allergens detected in a specific scan.
    /// </summary>
    public class ScanAllergy
    {
        public int ScanId { get; set; }
        public ScanHistory Scan { get; set; } = null!;

        public int AllergyId { get; set; }
        public Allergy Allergy { get; set; } = null!;

        /// <summary>User ID for implicit single-user model. Always set to 1 in single-user app.</summary>
        public int UserId { get; set; } = 1;

        /// <summary>Confidence level or severity of this specific allergen detection in this scan.</summary>
        public ScanFlag DetectionFlag { get; set; } = ScanFlag.Contains;

        /// <summary>Additional notes about this allergen detection (e.g., ingredient list position).</summary>
        public string? Notes { get; set; }
    }
}
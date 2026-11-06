namespace AllergyScan.Models
{
    /// <summary>One allergen that was detected in a scan (API junction row).</summary>
    public class ScanAllergy
    {
        public int AllergyId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>The user's severity preference for this allergy.</summary>
        public ScanFlag AllergyFlag { get; set; }

        /// <summary>The severity detected for this allergy in this specific scan.</summary>
        public ScanFlag DetectionFlag { get; set; }

        public string? Notes { get; set; }
    }
}

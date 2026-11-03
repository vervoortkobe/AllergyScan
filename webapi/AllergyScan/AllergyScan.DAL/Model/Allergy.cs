namespace AllergyScan.DAL
{
    public class Allergy
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Whether the user wants to avoid products that contain or may contain this allergen.</summary>
        public ScanFlag Flag { get; set; } = ScanFlag.Contains;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

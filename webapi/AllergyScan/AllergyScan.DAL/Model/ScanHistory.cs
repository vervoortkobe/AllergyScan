namespace AllergyScan.DAL
{
    public enum ScanFlag
    {
        NoAllergen = 0,
        AllergenDetected = 1
    }

    public class ScanHistory
    {
        public int Id { get; set; }

        /// <summary>The original picture. Stored as a BLOB in SQLite.</summary>
        public byte[]? Image { get; set; }

        public string? ImageContentType { get; set; }

        public string? ImageFileName { get; set; }

        /// <summary>The ingredient text produced by the app's OCR provider.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>The result of the app's allergen check.</summary>
        public ScanFlag Flag { get; set; }

        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    }
}

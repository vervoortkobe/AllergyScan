using System;

namespace AllergyScan.DAL.Model
{
    /// <summary>Represents a stored scan in the database.</summary>
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

        /// <summary>User ID for implicit single-user model. Always set to 1 in single-user app.</summary>
        public int UserId { get; set; } = 1;

        /// <summary>Collection of allergies detected in this scan.</summary>
        public ICollection<ScanAllergy> ScanAllergies { get; set; } = new List<ScanAllergy>();
    }
}

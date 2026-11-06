using System;

namespace AllergyScan.DAL.Model
{
    public class Allergy
    {
        public int Id { get; set; }
        
        /// <summary>User ID for implicit single-user model. Always set to 1 in single-user app.</summary>
        public int UserId { get; set; } = 1;
        
        public string Name { get; set; } = string.Empty;
        public ScanFlag Flag { get; set; } = ScanFlag.Contains;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>Collection of scan results where this allergen was detected.</summary>
        public ICollection<ScanAllergy> ScanAllergies { get; set; } = new List<ScanAllergy>();
    }
}

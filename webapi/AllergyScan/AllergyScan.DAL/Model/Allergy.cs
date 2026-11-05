using System;

namespace AllergyScan.DAL.Model
{
    public class Allergy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ScanFlag Flag { get; set; } = ScanFlag.Contains;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

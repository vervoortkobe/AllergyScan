namespace AllergyScan.DAL
{
    public class Language
    {
        public int Id { get; set; }

        /// <summary>ISO 639-1 code, for example "en".</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Human readable name, for example "English".</summary>
        public string Name { get; set; } = string.Empty;

        public ICollection<UserSettings> UserSettings { get; set; } = new List<UserSettings>();
    }
}

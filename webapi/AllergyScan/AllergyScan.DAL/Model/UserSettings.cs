namespace AllergyScan.DAL
{
    /// <summary>
    /// The application is single-user, so this table always contains exactly one row.
    /// </summary>
    public class UserSettings
    {
        public const int SingletonId = 1;

        public int Id { get; set; } = SingletonId;

        public int LanguageId { get; set; }

        public Language Language { get; set; } = null!;
    }
}

namespace AllergyScan.Services
{
    /// <summary>
    /// Centralized color definitions for allergen flag severities.
    /// Use these constants instead of hardcoding colors to maintain consistency.
    /// </summary>
    public static class FlagColors
    {
        // Background colors for each severity level.
        // Red for Contains, vivid orange for MayContain, vivid yellow for MayContainTraces.
        // All severity levels use black text for a consistent, high-contrast result page.
        public static readonly Color ContainsBackground = Color.FromArgb("#D32F2F");    // Strong red
        public static readonly Color MayContainBackground = Color.FromArgb("#FF9F1C"); // Vivid orange
        public static readonly Color MayContainTracesBackground = Color.FromArgb("#FFD60A"); // Vivid yellow
        public static readonly Color SafeBackground = Color.FromArgb("#D1FADF");

        // Text color (black for all severity levels, including Contains)
        public static readonly Color TextColor = Colors.Black;
    }
}

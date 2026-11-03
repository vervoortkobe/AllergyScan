using Microsoft.Maui.Graphics;

namespace AllergyScan.Services
{
    /// <summary>
    /// Centralized color definitions for allergen flag severities.
    /// Use these constants instead of hardcoding colors to maintain consistency.
    /// </summary>
    public static class FlagColors
    {
        // Background colors for each severity level
        // Using distinct, less vibrant colors for better visual differentiation
        // Red for Contains, Orange for MayContain, Yellow for MayContainTraces
        public static readonly Color ContainsBackground = Color.FromArgb("#D32F2F");    // Strong red
        public static readonly Color MayContainBackground = Color.FromArgb("#FFCC66"); // Orange (more distinct from red)
        public static readonly Color MayContainTracesBackground = Color.FromArgb("#FFFFCC"); // Light yellow
        public static readonly Color SafeBackground = Color.FromArgb("#D1FADF");

        // Text color (black for all severity levels)
        public static readonly Color TextColor = Colors.Black;
    }
}

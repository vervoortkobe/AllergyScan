using AllergyScan.Localization;

namespace AllergyScan.Models
{
    /// <summary>Result of comparing the ingredient text with the user's allergies.</summary>
    public enum ScanFlag
    {
        NoAllergen = 0,
        MayContainTraces = 1,
        MayContain = 2,
        Contains = 3
    }

    public static class ScanFlagExtensions
    {
        /// <summary>
        /// Cycles through the three severity levels the user can choose for an allergy:
        /// may contain traces → may contain → contains → may contain traces.
        /// </summary>
        public static ScanFlag NextSeverity(this ScanFlag flag) => flag switch
        {
            ScanFlag.Contains => ScanFlag.MayContainTraces,
            ScanFlag.MayContain => ScanFlag.Contains,
            _ => ScanFlag.MayContain
        };

        /// <summary>Short user-facing text for an allergy severity.</summary>
        public static string ToSeverityText(this ScanFlag flag) => flag switch
        {
            ScanFlag.MayContain => LocalizationService.Instance["Flag.Badge.MayContain"],
            ScanFlag.MayContainTraces => LocalizationService.Instance["Flag.Badge.MayContainTraces"],
            _ => LocalizationService.Instance["Flag.Badge.Contains"]
        };
    }
}

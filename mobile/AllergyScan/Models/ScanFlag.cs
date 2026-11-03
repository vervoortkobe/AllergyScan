using AllergyScan.Localization;

namespace AllergyScan.Models
{
    /// <summary>Result of comparing the ingredient text with the user's allergies.</summary>
    public enum ScanFlag
    {
        NoAllergen = 0,
        Contains = 1,
        MayContain = 2,
        MayContainTraces = 3
    }

    public static class ScanFlagExtensions
    {
        /// <summary>
        /// Cycles through the three severity levels the user can choose for an allergy:
        /// contains → may contain → may contain traces → contains.
        /// </summary>
        public static ScanFlag NextSeverity(this ScanFlag flag) => flag switch
        {
            ScanFlag.Contains => ScanFlag.MayContain,
            ScanFlag.MayContain => ScanFlag.MayContainTraces,
            _ => ScanFlag.Contains
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

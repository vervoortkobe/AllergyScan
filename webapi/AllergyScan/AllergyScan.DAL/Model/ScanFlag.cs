using System;

namespace AllergyScan.DAL.Model
{
    /// <summary>
    /// Represents the severity level of allergen detection or allergy preference.
    /// 
    /// Allergy.Flag: User's allergy severity preferences (e.g., "I'm allergic to peanuts - flag: Contains")
    /// Scan.Flag: Detected allergen severity (e.g., "Scan detected peanuts - flag: MayContain")
    /// 
    /// Same enum but different semantics - this is intentional for comparison and validation.
    /// </summary>
    public enum ScanFlag
    {
        /// <summary>No allergen detected or concern.</summary>
        NoAllergen = 0,

        /// <summary>Allergen definitively detected (strong reaction expected).</summary>
        Contains = 1,

        /// <summary>Allergen possibly detected (mild reaction possible).</summary>
        MayContain = 2,

        /// <summary>Allergen may contain traces (minimal risk, highly sensitive individuals only).</summary>
        MayContainTraces = 3
    }
}
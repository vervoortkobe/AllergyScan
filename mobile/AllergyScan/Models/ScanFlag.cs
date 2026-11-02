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
}

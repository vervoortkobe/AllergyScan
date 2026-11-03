namespace AllergyScan.Localization
{
    /// <summary>
    /// The common allergen shortcuts shown on the allergies screens. Key is the
    /// translation key for the button label, Name is the canonical English name that
    /// is stored in the allergy profile and translated by the app.
    /// </summary>
    public static class AllergenSuggestions
    {
        public static IReadOnlyList<(string Key, string Name)> All { get; } = new[]
        {
            ("Allergen.Milk", "Milk"),
            ("Allergen.Egg", "Egg"),
            ("Allergen.Peanut", "Peanut"),
            ("Allergen.Soy", "Soy"),
            ("Allergen.Gluten", "Gluten"),
            ("Allergen.Wheat", "Wheat"),
            ("Allergen.Fish", "Fish"),
            ("Allergen.Shellfish", "Shellfish"),
            ("Allergen.TreeNut", "Tree nut"),
            ("Allergen.Mustard", "Mustard"),
            ("Allergen.Sesame", "Sesame"),
            ("Allergen.Celery", "Celery")
        };
    }
}

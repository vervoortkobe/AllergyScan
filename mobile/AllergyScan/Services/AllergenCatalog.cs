namespace AllergyScan.Services
{
    /// <summary>
    /// Maps an allergen name to search terms in multiple languages.
    /// </summary>
    public static class AllergenCatalog
    {
        private static readonly Dictionary<string, string[]> Groups = new(StringComparer.OrdinalIgnoreCase)
        {
            ["milk"] = new[] { "milk", "melk", "lait", "milch", "leche" },
            ["egg"] = new[] { "egg", "eggs", "ei", "eieren", "oeuf", "oeufs", "ei", "huevo", "huevos" },
            ["peanut"] = new[] { "peanut", "peanuts", "pinda", "pindas", "arachide", "cacahuete", "cacahuetes", "erdnuss" },
            ["soy"] = new[] { "soy", "soja", "soya" },
            ["gluten"] = new[] { "gluten" },
            ["wheat"] = new[] { "wheat", "tarwe", "ble", "weizen", "trigo" },
            ["fish"] = new[] { "fish", "vis", "poisson", "fisch", "pescado" },
            ["shellfish"] = new[] { "shellfish", "schaaldieren", "crustaces", "schalentiere", "mariscos" },
            ["tree nut"] = new[] { "tree nut", "tree nuts", "noten", "noix", "nusse", "nueces", "almond", "walnut", "hazelnut", "cashew" },
            ["mustard"] = new[] { "mustard", "mosterd", "moutarde", "senf", "mostaza" },
            ["sesame"] = new[] { "sesame", "sesam", "sesamo" },
            ["celery"] = new[] { "celery", "selderij", "celeri", "sellerie", "apio" },
        };

        public static string[] GetSearchTerms(string allergyName)
        {
            if (string.IsNullOrWhiteSpace(allergyName))
            {
                return Array.Empty<string>();
            }

            var trimmed = allergyName.Trim();

            foreach (var group in Groups)
            {
                if (group.Key.Equals(trimmed, StringComparison.OrdinalIgnoreCase)
                    || group.Value.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                {
                    return group.Value;
                }
            }

            return new[] { trimmed };
        }
    }
}

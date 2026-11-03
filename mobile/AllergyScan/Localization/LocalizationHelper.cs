namespace AllergyScan.Localization
{
    /// <summary>
    /// Small helpers for values that need a translation but are built outside XAML,
    /// such as language names and the common allergen shortcuts.
    /// </summary>
    public static class LocalizationHelper
    {
        /// <summary>Localized name of a translation language, for example "Dutch" or "Nederlands".</summary>
        public static string LanguageName(string? code, string? fallback = null)
        {
            if (!string.IsNullOrWhiteSpace(code)
                && LocalizationService.Instance.TryGet($"Language.{code.Trim().ToLowerInvariant()}", out var name))
            {
                return name;
            }

            return string.IsNullOrWhiteSpace(fallback) ? code ?? string.Empty : fallback;
        }

        /// <summary>
        /// Localized display name for an allergy. The profile stores the canonical
        /// English name for the common allergens, so those can be translated; any
        /// custom entry is shown as the user typed it.
        /// </summary>
        public static string AllergenName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var trimmed = name.Trim();

            foreach (var (key, canonical) in AllergenSuggestions.All)
            {
                if (string.Equals(canonical, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return LocalizationService.Instance[key];
                }
            }

            return char.ToUpperInvariant(trimmed[0]) + (trimmed.Length > 1 ? trimmed[1..] : string.Empty);
        }
    }
}

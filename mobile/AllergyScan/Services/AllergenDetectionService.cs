using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AllergyScan.Models;

namespace AllergyScan.Services
{
    public interface IAllergenDetectionService
    {
        IReadOnlyList<Allergy> Detect(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies);
    }

    /// <summary>
    /// Checks the ingredient text against the user's allergy profile. Both the
    /// original OCR text and the translated text are checked, and common allergen
    /// names are recognized in English, Dutch, French, German and Spanish.
    /// </summary>
    public sealed class AllergenDetectionService : IAllergenDetectionService
    {
        public IReadOnlyList<Allergy> Detect(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies)
        {
            var haystacks = new[] { Normalize(originalText), Normalize(translatedText) }
                .Where(text => text.Length > 0)
                .Distinct()
                .ToArray();

            var matches = new List<Allergy>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var allergy in allergies)
            {
                if (string.IsNullOrWhiteSpace(allergy.Name) || !seen.Add(allergy.Name.Trim()))
                {
                    continue;
                }

                var searchTerms = AllergenCatalog.GetSearchTerms(allergy.Name);

                if (searchTerms.Any(term => haystacks.Any(text => ContainsTerm(text, term))))
                {
                    matches.Add(allergy);
                }
            }

            return matches;
        }

        private static bool ContainsTerm(string normalizedText, string term)
        {
            var normalizedTerm = Normalize(term);

            if (normalizedTerm.Length == 0)
            {
                return false;
            }

            var pattern = $@"(?<![a-z0-9]){Regex.Escape(normalizedTerm)}(?:s|es)?(?![a-z0-9])";
            return Regex.IsMatch(normalizedText, pattern, RegexOptions.IgnoreCase);
        }

        private static string Normalize(string value)
        {
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(char.IsLetterOrDigit(character)
                    ? char.ToLowerInvariant(character)
                    : ' ');
            }

            return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
        }
    }

    internal static class AllergenCatalog
    {
        // Each group contains synonyms for one allergen in several languages.
        // A user allergy that matches any term in a group is checked against all
        // terms of that group.
        private static readonly string[][] Groups =
        {
            new[] { "milk", "melk", "lait", "milch", "leche", "dairy", "zuivel", "lactose", "lactosa" },
            new[] { "egg", "eggs", "ei", "eieren", "oeuf", "oeufs", "huevo", "huevos", "eier" },
            new[] { "peanut", "peanuts", "pinda", "pindas", "arachide", "arachides", "erdnuss", "erdnuesse", "cacahuete", "cacahuetes" },
            new[] { "soy", "soya", "soja", "soybean", "soybeans", "sojaboon", "sojabonen", "sojalecithin", "sojalecitine" },
            new[] { "wheat", "tarwe", "blé", "ble", "weizen", "trigo", "gluten" },
            new[] { "fish", "vis", "poisson", "fisch", "pescado", "pescados" },
            new[] { "shellfish", "crustacean", "crustaceans", "schaaldier", "schaaldieren", "crustace", "crustaces", "krustentier", "krustentiere", "marisco", "mariscos" },
            new[] { "sesame", "sesam", "sesamo", "sésame" },
            new[] { "mustard", "mosterd", "moutarde", "senf", "mostaza" },
            new[] { "celery", "selder", "selderij", "céleri", "celeri", "sellerie", "apio" },
            new[] { "lupin", "lupine", "lupinemeel", "lupinenmehl", "altramuz" },
            new[] { "sulphite", "sulphites", "sulfite", "sulfites", "sulfiet", "sulfieten", "sulfito", "sulfitos" },
            new[] { "tree nut", "tree nuts", "noten", "noix", "nuss", "nuesse", "frutos secos", "almond", "amandel", "amande", "mandel", "almendra", "hazelnut", "hazelnoot", "noisette", "haselnuss", "avellana", "walnut", "walnoot", "noix", "walnuss", "nuez", "cashew", "cashewnoot", "noix de cajou", "cashewnuss", "anacardo", "pistachio", "pistache", "pistazie", "pistacho" }
        };

        public static IReadOnlyList<string> GetSearchTerms(string allergyName)
        {
            var normalized = Normalize(allergyName);

            foreach (var group in Groups)
            {
                if (group.Any(term => IsSameWord(Normalize(term), normalized)))
                {
                    return group;
                }
            }

            return new[] { allergyName };
        }

        private static bool IsSameWord(string left, string right)
        {
            return left == right
                || left == right + "s"
                || right == left + "s"
                || left == right + "es"
                || right == left + "es";
        }

        private static string Normalize(string value)
        {
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(char.IsLetterOrDigit(character)
                    ? char.ToLowerInvariant(character)
                    : ' ');
            }

            return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
        }
    }
}

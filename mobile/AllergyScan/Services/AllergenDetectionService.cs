using AllergyScan.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AllergyScan.Services
{
    /// <summary>Detects allergens and their severity in ingredient text.</summary>
    public interface IAllergenDetectionService
    {
        IReadOnlyList<(Allergy Allergy, ScanFlag Flag)> Detect(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies);

        IReadOnlyList<(Allergy Allergy, ScanFlag Flag)> DetectMayContain(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies);

        IReadOnlyList<(Allergy Allergy, ScanFlag Flag)> DetectMayContainTraces(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies);
    }

    /// <summary>
    /// Checks the ingredient text against the user's allergy profile. Both the
    /// original OCR text and the translated text are checked. Allergen names are
    /// recognized in English, Dutch, French, German and Spanish, plus any
    /// translations stored on the allergy. The severity (contains / may contain /
    /// may contain traces) is determined from the surrounding context.
    /// </summary>
    public sealed class AllergenDetectionService : IAllergenDetectionService
    {
        private static readonly string[] MayContainPhrases =
        [
            "may contain", "may contains", "could contain", "might contain", "may contain traces", "contains traces", "traces of",
            "kan bevatten", "kan sporen bevatten", "bevat sporen", "sporen van", "zou kunnen bevatten",
            "peut contenir", "peut-etre contenir", "traces de",
            "kann enthalten", "kann spuren enthalten", "spuren von", "enthält spuren",
            "puede contener", "puede contener trazas", "trazas de",
            "puo contenere", "potrebbe contenere", "tracce di"
        ];

        private static readonly string[] TraceIndicators =
        [
            "traces", "sporen", "tracce", "trazas", "spuren"
        ];

        public IReadOnlyList<(Allergy Allergy, ScanFlag Flag)> Detect(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies)
        {
            var haystacks = new[] { Normalize(originalText), Normalize(translatedText) }
                .Where(text => text.Length > 0)
                .Distinct()
                .ToArray();

            var results = new List<(Allergy Allergy, ScanFlag Flag)>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var allergy in allergies)
            {
                if (string.IsNullOrWhiteSpace(allergy.Name) || !seen.Add(allergy.Name.Trim()))
                {
                    continue;
                }

                var searchTerms = AllergenCatalog.GetSearchTerms(allergy.Name)
                    .Concat(
                    [
                        allergy.TranslationNl,
                        allergy.TranslationFr,
                        allergy.TranslationDe,
                        allergy.TranslationEs
                    ])
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var severity = ScanFlag.NoAllergen;

                foreach (var haystack in haystacks)
                {
                    foreach (var term in searchTerms)
                    {
                        if (!ContainsTerm(haystack, term))
                        {
                            continue;
                        }

                        var termSeverity = DetermineSeverity(haystack, term);
                        if (termSeverity > severity)
                        {
                            severity = termSeverity;
                        }
                    }
                }

                if (severity != ScanFlag.NoAllergen)
                {
                    results.Add((allergy, severity));
                }
            }

            return results;
        }

        public IReadOnlyList<(Allergy Allergy, ScanFlag Flag)> DetectMayContain(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies)
        {
            return Detect(originalText, translatedText, allergies)
                .Where(r => r.Flag == ScanFlag.MayContain)
                .ToList();
        }

        public IReadOnlyList<(Allergy Allergy, ScanFlag Flag)> DetectMayContainTraces(
            string originalText,
            string translatedText,
            IEnumerable<Allergy> allergies)
        {
            return Detect(originalText, translatedText, allergies)
                .Where(r => r.Flag == ScanFlag.MayContainTraces)
                .ToList();
        }

        private static ScanFlag DetermineSeverity(string normalizedText, string term)
        {
            var normalizedTerm = Normalize(term);
            if (normalizedTerm.Length == 0)
            {
                return ScanFlag.NoAllergen;
            }

            // Find all occurrences of the term.
            var matches = new List<int>();
            var idx = 0;
            while (true)
            {
                var pos = normalizedText.IndexOf(normalizedTerm, idx, StringComparison.OrdinalIgnoreCase);
                if (pos < 0)
                {
                    break;
                }
                matches.Add(pos);
                idx = pos + normalizedTerm.Length;
            }

            if (matches.Count == 0)
            {
                return ScanFlag.NoAllergen;
            }

            var maxFlag = ScanFlag.NoAllergen;

            foreach (var pos in matches)
            {
                // Look back up to 80 chars and forward up to 40 chars for a may-contain
                // indicator, without crossing a sentence boundary.
                var lookbackStart = Math.Max(0, pos - 80);
                var backward = normalizedText.Substring(lookbackStart, pos - lookbackStart);
                var lookforwardEnd = Math.Min(normalizedText.Length, pos + normalizedTerm.Length + 40);
                var forward = normalizedText.Substring(pos + normalizedTerm.Length,
                    lookforwardEnd - (pos + normalizedTerm.Length));

                var lastBoundary = Math.Max(
                    backward.LastIndexOf('.'),
                    Math.Max(backward.LastIndexOf(';'), Math.Max(backward.LastIndexOf('\n'), backward.LastIndexOf(':'))));
                var backContext = lastBoundary >= 0
                    ? backward.Substring(lastBoundary + 1)
                    : backward;

                var contextWindow = normalizedText.Substring(Math.Max(0, pos - 120), Math.Min(240, normalizedText.Length - Math.Max(0, pos - 120)));

                // Dutch "kan ... bevatten" puts the verb after the allergen list.
                var dutchMayContain = (backContext.Contains("kan", StringComparison.OrdinalIgnoreCase)
                    || forward.Contains("kan", StringComparison.OrdinalIgnoreCase)
                    || contextWindow.Contains("kan", StringComparison.OrdinalIgnoreCase))
                    && (backContext.Contains("bevatten", StringComparison.OrdinalIgnoreCase)
                    || forward.Contains("bevatten", StringComparison.OrdinalIgnoreCase)
                    || contextWindow.Contains("bevatten", StringComparison.OrdinalIgnoreCase));

                var frenchMayContain = (backContext.Contains("peut", StringComparison.OrdinalIgnoreCase)
                    || forward.Contains("peut", StringComparison.OrdinalIgnoreCase)
                    || contextWindow.Contains("peut", StringComparison.OrdinalIgnoreCase))
                    && contextWindow.Contains("contenir", StringComparison.OrdinalIgnoreCase);

                var germanMayContain = (backContext.Contains("kann", StringComparison.OrdinalIgnoreCase)
                    || forward.Contains("kann", StringComparison.OrdinalIgnoreCase)
                    || contextWindow.Contains("kann", StringComparison.OrdinalIgnoreCase))
                    && (contextWindow.Contains("enthalten", StringComparison.OrdinalIgnoreCase)
                    || contextWindow.Contains("spuren", StringComparison.OrdinalIgnoreCase));

                var spanishMayContain = (backContext.Contains("puede", StringComparison.OrdinalIgnoreCase)
                    || forward.Contains("puede", StringComparison.OrdinalIgnoreCase)
                    || contextWindow.Contains("puede", StringComparison.OrdinalIgnoreCase))
                    && contextWindow.Contains("contener", StringComparison.OrdinalIgnoreCase);
                var matchedPhrase = MayContainPhrases.FirstOrDefault(p =>
                    backContext.Contains(p, StringComparison.OrdinalIgnoreCase)
                    || forward.Contains(p, StringComparison.OrdinalIgnoreCase)
                    || contextWindow.Contains(p, StringComparison.OrdinalIgnoreCase));

                if (matchedPhrase is not null || dutchMayContain || frenchMayContain || germanMayContain || spanishMayContain)
                {
                    var combined = backContext + " " + forward + " " + contextWindow;
                    if (TraceIndicators.Any(t => combined.Contains(t, StringComparison.OrdinalIgnoreCase))
                        || (matchedPhrase is not null && TraceIndicators.Any(t => matchedPhrase.Contains(t, StringComparison.OrdinalIgnoreCase))))
                    {
                        if ((int)ScanFlag.MayContainTraces > (int)maxFlag)
                        {
                            maxFlag = ScanFlag.MayContainTraces;
                        }
                    }
                    else if ((int)ScanFlag.MayContain > (int)maxFlag)
                    {
                        maxFlag = ScanFlag.MayContain;
                    }
                }
                else if ((int)ScanFlag.Contains > (int)maxFlag)
                {
                    maxFlag = ScanFlag.Contains;
                }
            }

            return maxFlag;
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
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(char.ToLowerInvariant(character));
            }

            return builder.ToString();
        }
    }
}

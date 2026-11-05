using System.Text.Json;

namespace AllergyScan.Localization
{
    public interface ILocalizationService
    {
        string CurrentLanguageCode { get; }
        string this[string key] { get; }
        string Get(string key);
        bool TryGet(string key, out string value);
        string Format(string key, params object[] args);
        void SetLanguage(string? code);
        event EventHandler? LanguageChanged;
    }

    /// <summary>
    /// Loads the UI translations from the embedded translations.json file and keeps
    /// track of the language the user selected. English is the fallback for both the
    /// initial language and any missing translation.
    /// </summary>
    public sealed class LocalizationService : ILocalizationService
    {
        private const string DefaultLanguage = "en";
        private const string LanguagePreferenceKey = "LanguageCode";

        private static readonly string[] SupportedLanguages = { "en", "nl", "fr", "de", "es" };

        private readonly Dictionary<string, Dictionary<string, string>> translations;

        public static LocalizationService Instance { get; } = new();

        public event EventHandler? LanguageChanged;

        private LocalizationService()
        {
            translations = LoadTranslations();
            CurrentLanguageCode = Normalize(Preferences.Default.Get(LanguagePreferenceKey, DefaultLanguage));
        }

        public string CurrentLanguageCode { get; private set; }

        public string this[string key] => Get(key);

        public string Get(string key)
        {
            return TryGet(key, out var value) ? value : key;
        }

        public bool TryGet(string key, out string value)
        {
            if (translations.TryGetValue(CurrentLanguageCode, out var language)
                && language.TryGetValue(key, out value!))
            {
                return true;
            }

            if (translations.TryGetValue(DefaultLanguage, out var english)
                && english.TryGetValue(key, out value!))
            {
                return true;
            }

            value = key;
            return false;
        }

        public string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        public void SetLanguage(string? code)
        {
            var normalized = Normalize(code);

            if (string.Equals(CurrentLanguageCode, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            CurrentLanguageCode = normalized;
            Preferences.Default.Set(LanguagePreferenceKey, normalized);
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        private static string Normalize(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return DefaultLanguage;
            }

            var normalized = code.Trim().ToLowerInvariant();

            if (normalized.Length > 2)
            {
                normalized = normalized[..2];
            }

            return SupportedLanguages.Contains(normalized) ? normalized : DefaultLanguage;
        }

        private static Dictionary<string, Dictionary<string, string>> LoadTranslations()
        {
            var assembly = typeof(LocalizationService).Assembly;
            using var stream = assembly.GetManifestResourceStream("AllergyScan.Localization.translations.json");

            if (stream is null)
            {
                return new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var parsed = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json)
                ?? new Dictionary<string, Dictionary<string, string>>();

            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var (language, values) in parsed)
            {
                result[language] = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
            }

            return result;
        }
    }
}

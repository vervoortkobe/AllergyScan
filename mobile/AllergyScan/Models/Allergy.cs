using AllergyScan.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AllergyScan.Models
{
    public class Allergy : ObservableObject
    {
        private int id;
        private string name = string.Empty;
        private DateTime createdAt;

        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public DateTime CreatedAt
        {
            get => createdAt;
            set
            {
                if (SetProperty(ref createdAt, value))
                {
                    OnPropertyChanged(nameof(CreatedAtText));
                }
            }
        }

        private ScanFlag flag = ScanFlag.Contains;

        public ScanFlag Flag
        {
            get => flag;
            set
            {
                if (SetProperty(ref flag, value))
                {
                    OnPropertyChanged(nameof(FlagTextColor));
                    OnPropertyChanged(nameof(FlagBadge));
                    OnPropertyChanged(nameof(FlagBackgroundColor));
                    OnPropertyChanged(nameof(FlagStrokeColor));
                }
            }
        }

        public Color FlagTextColor => Flag switch
        {
            ScanFlag.Contains => Color.FromArgb("#B42318"),
            ScanFlag.MayContain => Color.FromArgb("#B54708"),
            ScanFlag.MayContainTraces => Color.FromArgb("#B8860B"),
            _ => Color.FromArgb("#6E6E6E")
        };

        public string FlagBadge => Flag switch
        {
            ScanFlag.Contains => LocalizationService.Instance["Flag.Badge.Contains"],
            ScanFlag.MayContain => LocalizationService.Instance["Flag.Badge.MayContain"],
            ScanFlag.MayContainTraces => LocalizationService.Instance["Flag.Badge.MayContainTraces"],
            _ => string.Empty
        };

        public Color FlagBackgroundColor => Flag switch
        {
            ScanFlag.Contains => Color.FromArgb("#FEE4E2"),
            ScanFlag.MayContain => Color.FromArgb("#FFFAEB"),
            ScanFlag.MayContainTraces => Color.FromArgb("#FFF8DC"),
            _ => Color.FromArgb("#F2F2F2")
        };

        public Color FlagStrokeColor => Flag switch
        {
            ScanFlag.Contains => Color.FromArgb("#B42318"),
            ScanFlag.MayContain => Color.FromArgb("#B54708"),
            ScanFlag.MayContainTraces => Color.FromArgb("#B8860B"),
            _ => Color.FromArgb("#ACACAC")
        };

        public string? TranslationNl { get; set; }
        public string? TranslationFr { get; set; }
        public string? TranslationDe { get; set; }
        public string? TranslationEs { get; set; }

        public string DisplayName
        {
            get
            {
                var currentLanguage = LocalizationService.Instance.CurrentLanguageCode;
                
                // Try to use the translation for the current language
                string? translation = currentLanguage switch
                {
                    "nl" => TranslationNl,
                    "fr" => TranslationFr,
                    "de" => TranslationDe,
                    "es" => TranslationEs,
                    _ => null
                };
                
                if (!string.IsNullOrWhiteSpace(translation))
                {
                    return translation;
                }
                
                // Fall back to AllergenSuggestions for common allergens
                return LocalizationHelper.AllergenName(Name);
            }
        }

        public string CreatedAtText => CreatedAt == default
            ? string.Empty
            : CreatedAt.ToLocalTime().ToString("d");
    }
}

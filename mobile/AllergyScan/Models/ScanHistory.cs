using CommunityToolkit.Mvvm.ComponentModel;

namespace AllergyScan.Models
{
    public class ScanHistory : ObservableObject
    {
        private int id;
        private string text = string.Empty;
        private ScanFlag flag;
        private DateTime scannedAt;
        private bool hasImage;
        private string? imageUrl;

        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        /// <summary>The ingredient text that was produced by OCR.</summary>
        public string Text
        {
            get => text;
            set
            {
                if (SetProperty(ref text, value))
                {
                    OnPropertyChanged(nameof(TextPreview));
                }
            }
        }

        public ScanFlag Flag
        {
            get => flag;
            set
            {
                if (SetProperty(ref flag, value))
                {
                    OnPropertyChanged(nameof(IsAllergen));
                    OnPropertyChanged(nameof(FlagText));
                    OnPropertyChanged(nameof(FlagTitle));
                    OnPropertyChanged(nameof(FlagColor));
                    OnPropertyChanged(nameof(FlagBackgroundColor));
                }
            }
        }

        public DateTime ScannedAt
        {
            get => scannedAt;
            set
            {
                if (SetProperty(ref scannedAt, value))
                {
                    OnPropertyChanged(nameof(ScannedAtText));
                }
            }
        }

        public bool HasImage
        {
            get => hasImage;
            set
            {
                if (SetProperty(ref hasImage, value))
                {
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        /// <summary>Relative URL returned by the API, for example /api/scans/1/image.</summary>
        public string? ImageUrl
        {
            get => imageUrl;
            set
            {
                if (SetProperty(ref imageUrl, value))
                {
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public bool IsAllergen => Flag is ScanFlag.Contains or ScanFlag.MayContain or ScanFlag.MayContainTraces;

        public bool IsMayContain => Flag is ScanFlag.MayContain or ScanFlag.MayContainTraces;

        public bool IsMayContainTraces => Flag == ScanFlag.MayContainTraces;

        public string FlagText => Flag switch
        {
            ScanFlag.Contains => "Contains allergen",
            ScanFlag.MayContain => "May contain allergen",
            ScanFlag.MayContainTraces => "May contain traces",
            _ => "No allergens detected"
        };

        public string FlagTitle => Flag switch
        {
            ScanFlag.Contains => "⚠ Allergen detected",
            ScanFlag.MayContain => "⚠ May contain allergen",
            ScanFlag.MayContainTraces => "⚠ May contain traces",
            _ => "✓ No allergens detected"
        };

        public Color FlagColor => Flag switch
        {
            ScanFlag.Contains => Color.FromArgb("#B42318"),
            ScanFlag.MayContain => Color.FromArgb("#B8860B"),
            ScanFlag.MayContainTraces => Color.FromArgb("#F4A460"),
            _ => Color.FromArgb("#067647")
        };

        public Color FlagBackgroundColor => Flag switch
        {
            ScanFlag.Contains => Color.FromArgb("#FEE4E2"),
            ScanFlag.MayContain => Color.FromArgb("#FFF8DC"),
            ScanFlag.MayContainTraces => Color.FromArgb("#FFE4B5"),
            _ => Color.FromArgb("#D1FADF")
        };

        public string ScannedAtText => ScannedAt == default
            ? string.Empty
            : ScannedAt.ToLocalTime().ToString("g");

        public string TextPreview => Text.Length <= 120
            ? Text
            : Text[..120] + "…";

        public ImageSource? ImageSource => HasImage && !string.IsNullOrWhiteSpace(ImageUrl)
            ? ImageSource.FromUri(new Uri(AppConfig.ApiBaseUrl.TrimEnd('/') + ImageUrl))
            : null;
    }
}

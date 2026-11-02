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

        private ScanFlag flag = ScanFlag.NoAllergen;

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
            ScanFlag.AllergenDetected => Color.FromArgb("#B42318"),
            ScanFlag.MayContain => Color.FromArgb("#B8860B"),
            _ => Color.FromArgb("#067647")
        };

        public string FlagBadge => Flag switch
        {
            ScanFlag.AllergenDetected => "contains",
            ScanFlag.MayContain => "may contain",
            _ => string.Empty
        };

        public Color FlagBackgroundColor => Flag switch
        {
            ScanFlag.AllergenDetected => Color.FromArgb("#FEE4E2"),
            ScanFlag.MayContain => Color.FromArgb("#FFF8DC"),
            _ => Color.FromArgb("#D1FADF")
        };

        public Color FlagStrokeColor => Flag switch
        {
            ScanFlag.AllergenDetected => Color.FromArgb("#B42318"),
            ScanFlag.MayContain => Color.FromArgb("#B8860B"),
            _ => Color.FromArgb("#067647")
        };

        public string DisplayName => string.IsNullOrWhiteSpace(Name)
            ? string.Empty
            : char.ToUpperInvariant(Name[0]) + (Name.Length > 1 ? Name[1..] : string.Empty);

        public string CreatedAtText => CreatedAt == default
            ? string.Empty
            : CreatedAt.ToLocalTime().ToString("d");
    }
}

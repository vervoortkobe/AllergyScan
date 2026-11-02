using AllergyScan.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class ResultViewModel : ObservableObject
    {
        public static ResultViewModel Current { get; private set; }
        public static ScanFlag PendingFlag { get; set; }
        public static string PendingAllergens { get; set; } = string.Empty;

        private ScanFlag flag;
        private string allergens = string.Empty;
        private bool isAllergen;
        private bool isMayContain;
        private bool isSafe;
        private string resultTitle = string.Empty;
        private string resultSubtitle = string.Empty;
        private Color resultColor;
        private Color resultBgColor;
        private string resultIcon = string.Empty;

        public ScanFlag Flag
        {
            get => flag;
            set => SetProperty(ref flag, value);
        }

        public string Allergens
        {
            get => allergens;
            set => SetProperty(ref allergens, value);
        }

        public bool HasAllergens => Flag != ScanFlag.NoAllergen && !string.IsNullOrWhiteSpace(allergens);

        public bool IsAllergen
        {
            get => isAllergen;
            set => SetProperty(ref isAllergen, value);
        }

        public bool IsMayContain
        {
            get => isMayContain;
            set => SetProperty(ref isMayContain, value);
        }

        public bool IsSafe
        {
            get => isSafe;
            set => SetProperty(ref isSafe, value);
        }

        public string ResultTitle
        {
            get => resultTitle;
            set => SetProperty(ref resultTitle, value);
        }

        public string ResultSubtitle
        {
            get => resultSubtitle;
            set => SetProperty(ref resultSubtitle, value);
        }

        public Color ResultColor
        {
            get => resultColor;
            set => SetProperty(ref resultColor, value);
        }

        public Color ResultBgColor
        {
            get => resultBgColor;
            set => SetProperty(ref resultBgColor, value);
        }

        public string ResultIcon
        {
            get => resultIcon;
            set => SetProperty(ref resultIcon, value);
        }

        public IAsyncRelayCommand BackCommand { get; }

        public ResultViewModel(INavigationService navigationService)
        {
            Current = this;
            BackCommand = new AsyncRelayCommand(navigationService.GoBackAsync);

            // Apply any pending result set before this page was created
            ApplyResult(PendingFlag, PendingAllergens);
            PendingFlag = ScanFlag.NoAllergen;
            PendingAllergens = string.Empty;
        }

        public static void SetPendingResult(ScanFlag flag, string allergens)
        {
            PendingFlag = flag;
            PendingAllergens = allergens ?? string.Empty;
        }

        public void ApplyResult(ScanFlag flag, string allergens)
        {
            Flag = flag;
            Allergens = allergens;

            switch (flag)
            {
                case ScanFlag.AllergenDetected:
                    IsAllergen = true;
                    IsMayContain = false;
                    IsSafe = false;
                    ResultTitle = "Allergen detected";
                    ResultSubtitle = $"This product contains: {allergens}";
                    ResultColor = Color.FromArgb("#B42318");
                    ResultBgColor = Color.FromArgb("#FEE4E2");
                    ResultIcon = "✕";
                    break;
                case ScanFlag.MayContain:
                    IsAllergen = false;
                    IsMayContain = true;
                    IsSafe = false;
                    ResultTitle = "May contain allergen";
                    ResultSubtitle = $"This product may contain: {allergens}";
                    ResultColor = Color.FromArgb("#B8860B");
                    ResultBgColor = Color.FromArgb("#FFF8DC");
                    ResultIcon = "!";
                    break;
                default:
                    IsAllergen = false;
                    IsMayContain = false;
                    IsSafe = true;
                    ResultTitle = "No allergens detected";
                    ResultSubtitle = "None of your allergies were found.";
                    ResultColor = Color.FromArgb("#067647");
                    ResultBgColor = Color.FromArgb("#D1FADF");
                    ResultIcon = "✓";
                    break;
            }
        }
    }
}

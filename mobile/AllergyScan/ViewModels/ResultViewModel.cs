using AllergyScan.Localization;
using AllergyScan.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class ResultViewModel : ObservableObject
    {
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
            BackCommand = new AsyncRelayCommand(navigationService.GoBackAsync);
        }

        private static string T(string key) => LocalizationService.Instance[key];

        public void ApplyResult(ScanFlag flag, string allergens)
        {
            Flag = flag;
            Allergens = allergens;

            switch (flag)
            {
                case ScanFlag.Contains:
                    IsAllergen = true;
                    IsMayContain = false;
                    IsSafe = false;
                    ResultTitle = T("Result.AllergenDetected");
                    ResultSubtitle = string.Format(T("Result.ContainsDescription"), allergens);
                    ResultColor = Colors.Black;
                    ResultBgColor = Color.FromArgb("#434343");
                    ResultIcon = "✕";
                    break;
                case ScanFlag.MayContain:
                    IsAllergen = false;
                    IsMayContain = true;
                    IsSafe = false;
                    ResultTitle = T("Result.MayContain");
                    ResultSubtitle = string.Format(T("Result.MayContainDescription"), allergens);
                    ResultColor = Colors.Black;
                    ResultBgColor = Color.FromArgb("#666666");
                    ResultIcon = "!";
                    break;
                case ScanFlag.MayContainTraces:
                    IsAllergen = false;
                    IsMayContain = true;
                    IsSafe = false;
                    ResultTitle = T("Result.MayContainTraces");
                    ResultSubtitle = string.Format(T("Result.MayContainTracesDescription"), allergens);
                    ResultColor = Colors.Black;
                    ResultBgColor = Color.FromArgb("#999999");
                    ResultIcon = "!";
                    break;
                default:
                    IsAllergen = false;
                    IsMayContain = false;
                    IsSafe = true;
                    ResultTitle = T("Result.NoAllergens");
                    ResultSubtitle = T("Result.NoneDescription");
                    ResultColor = Colors.Black;
                    ResultBgColor = Color.FromArgb("#D1FADF");
                    ResultIcon = "✓";
                    break;
            }
        }
    }
}

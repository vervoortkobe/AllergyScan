using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly ILanguageDataService languageDataService;
        private readonly IAllergyDataService allergyDataService;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;

        private List<Language> languages = new();
        private Language? selectedLanguage;
        private Language? currentLanguage;
        private ObservableCollection<string> allergies = new();
        private string customAllergyName = string.Empty;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public SettingsViewModel(
            ILanguageDataService languageDataService,
            IAllergyDataService allergyDataService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.languageDataService = languageDataService;
            this.allergyDataService = allergyDataService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            RefreshCommand = new AsyncRelayCommand(InitializeAsync);
            AddCustomAllergyCommand = new RelayCommand(AddCustomAllergy);
            AddSuggestionCommand = new RelayCommand<string>(AddSuggestion);
            RemoveAllergyCommand = new RelayCommand<string>(RemoveAllergy);
        }

        public List<string> CommonSuggestions { get; } = new()
        {
            "Milk", "Egg", "Peanut", "Soy", "Gluten", "Wheat", "Fish", "Shellfish",
            "Tree nut", "Mustard", "Sesame", "Celery"
        };

        public List<Language> Languages
        {
            get => languages;
            set
            {
                if (SetProperty(ref languages, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public Language? SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                if (SetProperty(ref selectedLanguage, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public Language? CurrentLanguage
        {
            get => currentLanguage;
            private set
            {
                if (SetProperty(ref currentLanguage, value))
                {
                    OnPropertyChanged(nameof(CurrentLanguageText));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public ObservableCollection<string> Allergies
        {
            get => allergies;
            set => SetProperty(ref allergies, value);
        }

        public string CustomAllergyName
        {
            get => customAllergyName;
            set
            {
                if (SetProperty(ref customAllergyName, value))
                {
                    OnPropertyChanged(nameof(CanAddCustom));
                }
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (SetProperty(ref isBusy, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                    OnPropertyChanged(nameof(CanAddCustom));
                }
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            private set
            {
                if (SetProperty(ref statusMessage, value))
                {
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public bool CanSave => !IsBusy && SelectedLanguage is not null && SelectedLanguage.Code != CurrentLanguage?.Code;

        public bool CanAddCustom => !IsBusy && !string.IsNullOrWhiteSpace(CustomAllergyName);

        public string ApiUrl => AppConfig.ApiBaseUrl;

        public string CurrentLanguageText => CurrentLanguage is null
            ? "Not loaded"
            : CurrentLanguage.DisplayName;

        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddCustomAllergyCommand { get; }
        public ICommand AddSuggestionCommand { get; }
        public ICommand RemoveAllergyCommand { get; }

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                var all = await languageDataService.GetAllAsync();
                Languages = all;
                CurrentLanguage = await languageDataService.GetCurrentAsync();
                SelectedLanguage = Languages.FirstOrDefault(l => l.Code == CurrentLanguage?.Code);

                try
                {
                    var existingAllergies = await allergyDataService.GetAllAsync();
                    Allergies = new ObservableCollection<string>(
                        existingAllergies
                            .Select(a => a.Name)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase));
                }
                catch
                {
                    Allergies = new ObservableCollection<string>();
                }

                StatusMessage = string.Empty;
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            if (!CanSave || SelectedLanguage is null)
                return;

            IsBusy = true;
            try
            {
                CurrentLanguage = await languageDataService.SetCurrentAsync(SelectedLanguage.Code);
                WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(CurrentLanguage));
                Preferences.Default.Set("LanguageConfigured", true);
                await SaveAllergiesAsync();
                Preferences.Default.Set("HasCompletedInitialSetup", true);
                StatusMessage = $"Language changed to {CurrentLanguage.Name}";
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddSuggestion(string? suggestion)
        {
            if (string.IsNullOrWhiteSpace(suggestion))
                return;

            var trimmed = suggestion.Trim();
            if (!Allergies.Any(a => string.Equals(a, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                Allergies.Add(trimmed);
            }
        }

        private void AddCustomAllergy()
        {
            if (!CanAddCustom)
                return;

            var trimmed = CustomAllergyName.Trim();
            if (!Allergies.Any(a => string.Equals(a, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                Allergies.Add(trimmed);
            }
            CustomAllergyName = string.Empty;
        }

        private void RemoveAllergy(string? allergy)
        {
            if (string.IsNullOrWhiteSpace(allergy))
                return;

            Allergies.Remove(allergy);
        }

        private async Task SaveAllergiesAsync()
        {
            var existing = await allergyDataService.GetAllAsync();
            var existingNames = new HashSet<string>(existing.Select(a => a.Name.Trim()), StringComparer.OrdinalIgnoreCase);

            foreach (var allergyName in Allergies)
            {
                if (!existingNames.Contains(allergyName))
                {
                    try
                    {
                        await allergyDataService.CreateAsync(allergyName);
                    }
                    catch
                    {
                        // Ignore duplicates or minor creation conflicts
                    }
                }
            }
        }
    }
}

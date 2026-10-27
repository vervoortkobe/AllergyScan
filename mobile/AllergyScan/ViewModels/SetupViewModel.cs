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
    public class SetupViewModel : ObservableObject
    {
        private readonly ILanguageDataService languageDataService;
        private readonly IAllergyDataService allergyDataService;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;

        private List<Language> languages = new();
        private Language? selectedLanguage;
        private ObservableCollection<string> allergies = new();
        private string customAllergyName = string.Empty;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public SetupViewModel(
            ILanguageDataService languageDataService,
            IAllergyDataService allergyDataService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.languageDataService = languageDataService;
            this.allergyDataService = allergyDataService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            CommonSuggestions = new List<string>
            {
                "Milk",
                "Egg",
                "Peanut",
                "Soy",
                "Gluten",
                "Wheat",
                "Fish",
                "Shellfish",
                "Tree nut",
                "Mustard",
                "Sesame",
                "Celery"
            };

            AddCustomAllergyCommand = new RelayCommand(AddCustomAllergy);
            AddSuggestionCommand = new RelayCommand<string>(AddSuggestion);
            RemoveAllergyCommand = new RelayCommand<string>(RemoveAllergy);
            CompleteSetupCommand = new AsyncRelayCommand(CompleteSetupAsync);
            SkipCommand = new AsyncRelayCommand(SkipAsync);
        }

        public List<string> CommonSuggestions { get; }

        public List<Language> Languages
        {
            get => languages;
            set => SetProperty(ref languages, value);
        }

        public Language? SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                if (SetProperty(ref selectedLanguage, value))
                {
                    OnPropertyChanged(nameof(CanComplete));
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
                    OnPropertyChanged(nameof(CanComplete));
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

        public bool CanAddCustom => !IsBusy && !string.IsNullOrWhiteSpace(CustomAllergyName);

        public bool CanComplete => !IsBusy && SelectedLanguage is not null;

        public ICommand AddCustomAllergyCommand { get; }
        public ICommand AddSuggestionCommand { get; }
        public ICommand RemoveAllergyCommand { get; }
        public ICommand CompleteSetupCommand { get; }
        public ICommand SkipCommand { get; }

        public async Task InitializeAsync()
        {
            IsBusy = true;
            StatusMessage = "Loading languages and existing settings…";

            try
            {
                var availableLanguages = await languageDataService.GetAllAsync();
                Languages = availableLanguages;

                try
                {
                    var current = await languageDataService.GetCurrentAsync();
                    SelectedLanguage = Languages.FirstOrDefault(l => l.Code == current.Code)
                        ?? Languages.FirstOrDefault();
                }
                catch
                {
                    SelectedLanguage = Languages.FirstOrDefault();
                }

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

        private async Task CompleteSetupAsync()
        {
            if (SelectedLanguage is null || IsBusy)
                return;

            IsBusy = true;
            StatusMessage = "Saving your profile…";

            try
            {
                // 1. Save language
                var updatedLanguage = await languageDataService.SetCurrentAsync(SelectedLanguage.Code);
                WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(updatedLanguage));
                Preferences.Default.Set("LanguageConfigured", true);

                // 2. Sync allergies with backend
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

                Preferences.Default.Set("HasCompletedInitialSetup", true);
                WeakReferenceMessenger.Default.Send(new RefreshScansMessage());

                StatusMessage = "Setup complete!";
                await Shell.Current.GoToAsync("//scan");
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync("Setup Error", exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SkipAsync()
        {
            if (SelectedLanguage is not null)
            {
                try
                {
                    var updatedLanguage = await languageDataService.SetCurrentAsync(SelectedLanguage.Code);
                    WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(updatedLanguage));
                }
                catch
                {
                    // Ignore if offline
                }
            }

            Preferences.Default.Set("HasCompletedInitialSetup", true);
            Preferences.Default.Set("LanguageConfigured", true);
            await Shell.Current.GoToAsync("//scan");
        }
    }
}

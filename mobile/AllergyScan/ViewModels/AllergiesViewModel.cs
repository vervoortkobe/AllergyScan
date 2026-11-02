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
    public class AllergiesViewModel : ObservableObject
    {
        private readonly IAllergyDataService allergyDataService;
        private readonly ILanguageDataService languageDataService;
        private readonly IDialogService dialogService;

        private ObservableCollection<Allergy> allergies = new();
        private Allergy? selectedAllergy;
        private string newAllergyName = string.Empty;
        private List<Language> languages = new();
        private Language? selectedLanguage;
        private Language? currentLanguage;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public AllergiesViewModel(
            IAllergyDataService allergyDataService,
            ILanguageDataService languageDataService,
            IDialogService dialogService)
        {
            this.allergyDataService = allergyDataService;
            this.languageDataService = languageDataService;
            this.dialogService = dialogService;

            AddCommand = new AsyncRelayCommand(AddAsync);
            DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedAsync);
            AddSuggestionCommand = new RelayCommand<string>(AddSuggestion);
            RemoveAllergyCommand = new AsyncRelayCommand<Allergy>(RemoveAllergyAsync);
            ToggleFlagCommand = new AsyncRelayCommand<Allergy>(ToggleFlagAsync);

            allergies.CollectionChanged += OnAllergiesChanged;
        }

        private void OnAllergiesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CountText));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(HasAllergies));
        }

        public List<string> CommonSuggestions { get; } = new()
        {
            "Milk", "Egg", "Peanut", "Soy", "Gluten", "Wheat", "Fish", "Shellfish",
            "Tree nut", "Mustard", "Sesame", "Celery"
        };

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
                if (SetProperty(ref selectedLanguage, value) && selectedLanguage is not null)
                {
                    _ = SetLanguageAsync(selectedLanguage.Code);
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
                }
            }
        }

        public string CurrentLanguageText => CurrentLanguage is null
            ? "Not loaded"
            : CurrentLanguage.DisplayName;

        public ObservableCollection<Allergy> Allergies
        {
            get => allergies;
            set
            {
                if (SetProperty(ref allergies, value))
                {
                    allergies.CollectionChanged += OnAllergiesChanged;
                    OnPropertyChanged(nameof(CountText));
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(HasAllergies));
                }
            }
        }

        public Allergy? SelectedAllergy
        {
            get => selectedAllergy;
            set
            {
                if (SetProperty(ref selectedAllergy, value))
                {
                    OnPropertyChanged(nameof(CanDeleteSelected));
                }
            }
        }

        public string NewAllergyName
        {
            get => newAllergyName;
            set
            {
                if (SetProperty(ref newAllergyName, value))
                {
                    OnPropertyChanged(nameof(CanAdd));
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
                    OnPropertyChanged(nameof(CanAdd));
                    OnPropertyChanged(nameof(CanDeleteSelected));
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

        public string CountText => Allergies.Count switch
        {
            0 => "0 allergies in your profile",
            1 => "1 allergy in your profile",
            var count => $"{count} allergies in your profile"
        };

        public bool CanAdd => !IsBusy && !string.IsNullOrWhiteSpace(NewAllergyName);
        public bool CanDeleteSelected => !IsBusy && SelectedAllergy is not null;

        public bool IsEmpty => Allergies.Count == 0;

        public bool HasAllergies => Allergies.Count > 0;

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public ICommand AddCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand AddSuggestionCommand { get; }
        public ICommand RemoveAllergyCommand { get; }
        public ICommand ToggleFlagCommand { get; }

        public async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Loading…";

                var (allergiesResult, languagesResult, current) = await LoadAllAsync();

                Allergies = new ObservableCollection<Allergy>(
                    allergiesResult.OrderBy(a => a.Name));
                Languages = languagesResult;
                CurrentLanguage = current;
                SelectedLanguage = Languages.FirstOrDefault(l =>
                    l.Code == CurrentLanguage?.Code) ?? Languages.FirstOrDefault();

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

        private async Task<(List<Allergy>, List<Language>, Language?)> LoadAllAsync()
        {
            var allergies = await allergyDataService.GetAllAsync();
            var languages = await languageDataService.GetAllAsync();
            var current = await languageDataService.GetCurrentAsync();
            return (allergies, languages, current);
        }

        private async Task SetLanguageAsync(string code)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            IsBusy = true;
            try
            {
                var updated = await languageDataService.SetCurrentAsync(code);
                CurrentLanguage = updated;
                WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(updated));
                Preferences.Default.Set("LanguageConfigured", true);
                Preferences.Default.Set("HasCompletedInitialSetup", true);
                StatusMessage = $"Language set to {updated.Name}";
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

        private async Task AddAsync()
        {
            var name = NewAllergyName.Trim();
            if (string.IsNullOrEmpty(name) || IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var allergy = await allergyDataService.CreateAsync(name);
                Allergies.Add(allergy);
                OnPropertyChanged(nameof(CountText));
                NewAllergyName = string.Empty;
                SelectedAllergy = allergy;
                StatusMessage = $"{allergy.Name} added to allergies.";
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
            if (string.IsNullOrWhiteSpace(suggestion) || IsBusy)
            {
                return;
            }

            var trimmed = suggestion.Trim();
            if (Allergies.Any(a => string.Equals(a.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            AddNewAllergyAsync(trimmed);
        }

        private async void AddNewAllergyAsync(string name)
        {
            IsBusy = true;
            try
            {
                var allergy = await allergyDataService.CreateAsync(name);
                Allergies.Add(allergy);
                OnPropertyChanged(nameof(CountText));
                StatusMessage = $"{allergy.Name} added to allergies.";
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

        private async Task DeleteSelectedAsync()
        {
            if (SelectedAllergy is null || IsBusy)
            {
                return;
            }

            var allergy = SelectedAllergy;
            IsBusy = true;
            try
            {
                await allergyDataService.DeleteAsync(allergy.Id);
                Allergies.Remove(allergy);
                OnPropertyChanged(nameof(CountText));
                SelectedAllergy = null;
                StatusMessage = $"{allergy.Name} removed from allergies.";
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

        private async Task RemoveAllergyAsync(Allergy? allergy)
        {
            if (allergy is null || IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                await allergyDataService.DeleteAsync(allergy.Id);
                Allergies.Remove(allergy);
                OnPropertyChanged(nameof(CountText));
                StatusMessage = $"{allergy.Name} removed from allergies.";
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

        private async Task ToggleFlagAsync(Allergy? allergy)
        {
            if (allergy is null || IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var next = allergy.Flag switch
                {
                    ScanFlag.NoAllergen => ScanFlag.AllergenDetected,
                    ScanFlag.AllergenDetected => ScanFlag.MayContain,
                    _ => ScanFlag.NoAllergen
                };

                allergy.Flag = next;
                await allergyDataService.UpdateAsync(allergy.Id, allergy.Name, next);

                var text = next switch
                {
                    ScanFlag.AllergenDetected => "contains",
                    ScanFlag.MayContain => "may contain",
                    _ => "none"
                };
                StatusMessage = $"{allergy.Name} set to {text}.";
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
    }
}

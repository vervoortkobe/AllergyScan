using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AllergyScan.Models;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class AllergiesViewModel : ObservableObject
    {
        private readonly IAllergyDataService allergyDataService;
        private readonly ITranslationService translationService;
        private readonly IDialogService dialogService;

        private ObservableCollection<Allergy> allergies = new();
        private Allergy? selectedAllergy;
        private string newAllergyName = string.Empty;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public AllergiesViewModel(
            IAllergyDataService allergyDataService,
            ITranslationService translationService,
            IDialogService dialogService)
        {
            this.allergyDataService = allergyDataService;
            this.translationService = translationService;
            this.dialogService = dialogService;

            AddCommand = new AsyncRelayCommand(AddAsync);
            DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedAsync);
            AddSuggestionCommand = new RelayCommand<string>(AddSuggestion);
            RemoveAllergyCommand = new AsyncRelayCommand<Allergy>(RemoveAllergyAsync);
            ToggleFlagCommand = new AsyncRelayCommand<Allergy>(ToggleFlagAsync);
        }

        public List<string> CommonSuggestions { get; } = new()
        {
            "Milk", "Egg", "Peanut", "Soy", "Gluten", "Wheat", "Fish", "Shellfish",
            "Tree nut", "Mustard", "Sesame", "Celery"
        };

        public ObservableCollection<Allergy> Allergies
        {
            get => allergies;
            set
            {
                if (SetProperty(ref allergies, value))
                {
                    OnPropertyChanged(nameof(CountText));
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

                var result = await allergyDataService.GetAllAsync();
                Allergies = new ObservableCollection<Allergy>(result.OrderBy(a => a.Name));

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

        public void AddSuggestion(string? suggestion)
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
                var nl = await TranslateAsync(name, "nl");
                var fr = await TranslateAsync(name, "fr");
                var de = await TranslateAsync(name, "de");
                var es = await TranslateAsync(name, "es");

                var allergy = await allergyDataService.CreateAsync(name, nl, fr, de, es);
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

        private async Task<string> TranslateAsync(string text, string toLanguage)
        {
            try
            {
                var result = await translationService.TranslateAsync(text, toLanguage);
                return result.Text;
            }
            catch
            {
                return text;
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

        public void ToggleFlag(Allergy allergy)
        {
            _ = ToggleFlagAsync(allergy);
        }

        public void RemoveAllergy(Allergy allergy)
        {
            _ = RemoveAllergyAsync(allergy);
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
                    ScanFlag.NoAllergen => ScanFlag.Contains,
                    ScanFlag.Contains => ScanFlag.MayContain,
                    ScanFlag.MayContain => ScanFlag.MayContainTraces,
                    _ => ScanFlag.NoAllergen
                };

                allergy.Flag = next;
                await allergyDataService.UpdateAsync(allergy.Id, allergy.Name, next);

                var text = next switch
                {
                    ScanFlag.Contains => "contains",
                    ScanFlag.MayContain => "may contain",
                    ScanFlag.MayContainTraces => "may contain traces",
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

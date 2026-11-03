using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AllergyScan.Localization;
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
            0 => T("Allergies.Count.Zero"),
            1 => T("Allergies.Count.One"),
            var count => string.Format(T("Allergies.Count.Many"), count)
        };

        private static string T(string key) => LocalizationService.Instance[key];

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
                StatusMessage = T("Allergies.Loading");

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
                var nl = await TranslateAsync(name, "nl");
                var fr = await TranslateAsync(name, "fr");
                var de = await TranslateAsync(name, "de");
                var es = await TranslateAsync(name, "es");

                var allergy = await allergyDataService.CreateAsync(name, nl, fr, de, es);
                Allergies.Add(allergy);
                OnPropertyChanged(nameof(CountText));
                NewAllergyName = string.Empty;
                SelectedAllergy = allergy;
                StatusMessage = string.Format(T("Allergies.Added"), LocalizationHelper.AllergenName(allergy.Name));
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
                StatusMessage = string.Format(T("Allergies.Added"), LocalizationHelper.AllergenName(allergy.Name));
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
                StatusMessage = string.Format(T("Allergies.Removed"), LocalizationHelper.AllergenName(allergy.Name));
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
                StatusMessage = string.Format(T("Allergies.Removed"), LocalizationHelper.AllergenName(allergy.Name));
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
            var previous = allergy.Flag;
            try
            {
                var next = previous.NextSeverity();

                allergy.Flag = next;
                await allergyDataService.UpdateAsync(allergy.Id, allergy.Name, next);

                StatusMessage = string.Format(
                    T("Allergies.SeveritySet"),
                    LocalizationHelper.AllergenName(allergy.Name),
                    next.ToSeverityText());
            }
            catch (Exception exception)
            {
                // Keep the badge in sync with the server if the update was rejected.
                allergy.Flag = previous;
                StatusMessage = exception.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

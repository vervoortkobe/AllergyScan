using AllergyScan.Localization;
using AllergyScan.Interfaces;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AllergyScan.ViewModels
{
    /// <summary>View model for the allergy setup page.</summary>
    public class SetupAllergiesViewModel : ObservableObject, ISetupAllergiesViewModel
    {
        private readonly IAllergyDataService allergyDataService;
        private readonly ITranslationService translationService;
        private readonly IHistoryRescanService historyRescanService;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;

        private ObservableCollection<Allergy> allergies = new();
        private string newAllergyName = string.Empty;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public SetupAllergiesViewModel(
            IAllergyDataService allergyDataService,
            ITranslationService translationService,
            IHistoryRescanService historyRescanService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.allergyDataService = allergyDataService;
            this.translationService = translationService;
            this.historyRescanService = historyRescanService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            AddCommand = new AsyncRelayCommand(AddAsync);
            AddSuggestionCommand = new RelayCommand<string>(AddSuggestion);
            RemoveAllergyCommand = new AsyncRelayCommand<Allergy>(RemoveAllergyAsync);
            ToggleFlagCommand = new AsyncRelayCommand<Allergy>(ToggleFlagAsync);
            CompleteSetupCommand = new AsyncRelayCommand(CompleteSetupAsync);
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

        public string CountText => Allergies.Count switch
        {
            0 => T("Setup.Allergies.Count.Zero"),
            1 => T("Setup.Allergies.Count.One"),
            var count => string.Format(T("Setup.Allergies.Count.Many"), count)
        };

        private static string T(string key) => LocalizationService.Instance[key];

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (SetProperty(ref isBusy, value))
                {
                    OnPropertyChanged(nameof(CanAdd));
                    OnPropertyChanged(nameof(CanComplete));
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

        public bool CanAdd => !IsBusy && !string.IsNullOrWhiteSpace(NewAllergyName);
        public bool CanComplete => !IsBusy;

        public ICommand AddCommand { get; }
        public ICommand AddSuggestionCommand { get; }
        public IAsyncRelayCommand<Allergy> RemoveAllergyCommand { get; }
        public IAsyncRelayCommand<Allergy> ToggleFlagCommand { get; }
        public IAsyncRelayCommand CompleteSetupCommand { get; }

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
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
                StatusMessage = string.Format(T("Allergies.Added"), LocalizationHelper.AllergenName(allergy.Name));

                // Re-scan history to update flags with the new allergen
                await historyRescanService.RescanAllHistoryAsync(Allergies);
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

                // Re-scan history to update flags with the new allergen
                await historyRescanService.RescanAllHistoryAsync(Allergies);
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

                // Re-scan history to update flags with the new allergen list
                await historyRescanService.RescanAllHistoryAsync(Allergies);
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

        public async Task ToggleFlagAsync(Allergy? allergy)
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

                // Re-scan history to update flags with the new allergen severity
                await historyRescanService.RescanAllHistoryAsync(Allergies);
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

        private async Task CompleteSetupAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                Preferences.Default.Set("HasCompletedInitialSetup", true);

                // Re-scan history to ensure all scans use the final allergy profile
                await historyRescanService.RescanAllHistoryAsync(Allergies);

                WeakReferenceMessenger.Default.Send(new RefreshScansMessage());

                StatusMessage = T("Setup.Allergies.Done");

                // Rebuild the shell so the tab titles use the language that was
                // picked during setup.
                if (Application.Current is App app)
                {
                    app.ReloadShell("//scan");
                }
                else
                {
                    await navigationService.NavigateToAsync("//scan");
                }
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync(T("Setup.Allergies.ErrorTitle"), exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

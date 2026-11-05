using AllergyScan.Localization;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace AllergyScan.ViewModels
{
    /// <summary>View model for the settings page.</summary>
    public class SettingsViewModel : ObservableObject
    {
        private readonly ILanguageDataService languageDataService;
        private readonly ILocalizationService localizationService;
        private readonly IDialogService dialogService;

        private List<Language> languages = new();
        private Language? selectedLanguage;
        private Language? currentLanguage;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public SettingsViewModel(
            ILanguageDataService languageDataService,
            ILocalizationService localizationService,
            IDialogService dialogService)
        {
            this.languageDataService = languageDataService;
            this.localizationService = localizationService;
            this.dialogService = dialogService;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            RefreshCommand = new AsyncRelayCommand(InitializeAsync);
        }

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

        public string CurrentLanguageText => CurrentLanguage is null
            ? T("Settings.NotLoaded")
            : CurrentLanguage.LocalizedDisplayName;

        public bool IsBusy
        {
            get => isBusy;
            private set => SetProperty(ref isBusy, value);
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

        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }

        private static string T(string key) => LocalizationService.Instance[key];

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                var all = await languageDataService.GetAllAsync();
                Languages = all;
                CurrentLanguage = await languageDataService.GetCurrentAsync();
                SelectedLanguage = Languages.FirstOrDefault(l => l.Code == CurrentLanguage?.Code);
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
            {
                return;
            }

            IsBusy = true;
            try
            {
                CurrentLanguage = await languageDataService.SetCurrentAsync(SelectedLanguage.Code);
                localizationService.SetLanguage(CurrentLanguage.Code);
                WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(CurrentLanguage));
                Preferences.Default.Set("LanguageConfigured", true);
                Preferences.Default.Set("HasCompletedInitialSetup", true);
                StatusMessage = string.Format(T("Settings.Changed"), CurrentLanguage.LocalizedDisplayName);
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
                return;
            }
            finally
            {
                IsBusy = false;
            }

            // Rebuild the shell so every page (and the tab titles) uses the new language.
            if (Application.Current is App app)
            {
                app.ReloadShell("//settings");
            }
        }
    }
}

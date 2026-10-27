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
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;

        private List<Language> languages = new();
        private Language? selectedLanguage;
        private Language? currentLanguage;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public SettingsViewModel(
            ILanguageDataService languageDataService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.languageDataService = languageDataService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            RefreshCommand = new AsyncRelayCommand(InitializeAsync);
            OpenSetupCommand = new AsyncRelayCommand(OpenSetupAsync);
        }

        public string ApiUrl => AppConfig.ApiBaseUrl;

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

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (SetProperty(ref isBusy, value))
                {
                    OnPropertyChanged(nameof(CanSave));
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

        public string CurrentLanguageText => CurrentLanguage is null
            ? "Not loaded"
            : CurrentLanguage.DisplayName;

        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenSetupCommand { get; }

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
                return;

            IsBusy = true;
            try
            {
                CurrentLanguage = await languageDataService.SetCurrentAsync(SelectedLanguage.Code);
                WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(CurrentLanguage));
                Preferences.Default.Set("LanguageConfigured", true);
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

        private async Task OpenSetupAsync()
        {
            await navigationService.NavigateToAsync("setup");
        }
    }
}

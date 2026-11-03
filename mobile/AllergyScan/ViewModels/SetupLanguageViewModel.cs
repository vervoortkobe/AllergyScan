using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AllergyScan.Localization;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class SetupLanguageViewModel : ObservableObject
    {
        private readonly ILanguageDataService languageDataService;
        private readonly IDialogService dialogService;

        private List<Language> languages = new();
        private Language? selectedLanguage;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public SetupLanguageViewModel(
            ILanguageDataService languageDataService,
            IDialogService dialogService)
        {
            this.languageDataService = languageDataService;
            this.dialogService = dialogService;

            NextCommand = new AsyncRelayCommand(NextAsync);
        }

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
                    OnPropertyChanged(nameof(CanNext));
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
                    OnPropertyChanged(nameof(CanNext));
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

        public bool CanNext => !IsBusy && SelectedLanguage is not null;

        public IAsyncRelayCommand NextCommand { get; }

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                var all = await languageDataService.GetAllAsync();
                Languages = all;

                try
                {
                    var current = await languageDataService.GetCurrentAsync();
                    SelectedLanguage = Languages.FirstOrDefault(l => l.Code == current.Code)
                        ?? DefaultLanguage();
                }
                catch
                {
                    SelectedLanguage = DefaultLanguage();
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

        /// <summary>English is the default when no language has been chosen yet.</summary>
        private Language? DefaultLanguage()
        {
            return Languages.FirstOrDefault(l => string.Equals(l.Code, "en", StringComparison.OrdinalIgnoreCase))
                ?? Languages.FirstOrDefault();
        }

        private async Task NextAsync()
        {
            if (SelectedLanguage is null || IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var updated = await languageDataService.SetCurrentAsync(SelectedLanguage.Code);

                // This page stays English, but the rest of the app switches to the
                // selected language from the next page onwards.
                LocalizationService.Instance.SetLanguage(updated.Code);

                WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(updated));
                Preferences.Default.Set("LanguageConfigured", true);
                StatusMessage = "Language saved";
                await Shell.Current.GoToAsync("setup-allergies");
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
    }
}

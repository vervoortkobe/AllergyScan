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
    public class SetupAllergiesViewModel : ObservableObject
    {
        private readonly IAllergyDataService allergyDataService;
        private readonly IDialogService dialogService;

        private ObservableCollection<Allergy> allergies = new();
        private bool isBusy;
        private string statusMessage = string.Empty;

        public SetupAllergiesViewModel(
            IAllergyDataService allergyDataService,
            IDialogService dialogService)
        {
            this.allergyDataService = allergyDataService;
            this.dialogService = dialogService;

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

        public string CountText => Allergies.Count switch
        {
            0 => "No allergies added yet",
            1 => "1 allergy",
            var count => $"{count} allergies"
        };

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (SetProperty(ref isBusy, value))
                {
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

        public bool CanComplete => !IsBusy;

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

        public async Task ToggleFlagAsync(Allergy? allergy)
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
    }
}

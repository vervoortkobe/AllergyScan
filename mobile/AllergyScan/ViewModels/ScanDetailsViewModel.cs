using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    public class ScanDetailsViewModel : ObservableRecipient, IRecipient<ScanSelectedMessage>
    {
        private readonly IScanDataService scanDataService;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private ScanHistory? scan;
        private string editText = string.Empty;
        private ScanFlag selectedFlag;
        private bool isBusy;
        private string statusMessage = string.Empty;

        public ScanDetailsViewModel(
            IScanDataService scanDataService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            this.scanDataService = scanDataService;
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
            CancelCommand = new AsyncRelayCommand(navigationService.GoBackAsync);

            Messenger.Register<ScanDetailsViewModel, ScanSelectedMessage>(
                this,
                static (recipient, message) => recipient.Receive(message));
        }

        public ScanHistory? Scan
        {
            get => scan;
            private set
            {
                if (SetProperty(ref scan, value))
                {
                    OnPropertyChanged(nameof(HasScan));
                    OnPropertyChanged(nameof(HasImage));
                    OnPropertyChanged(nameof(ScanImage));
                    OnPropertyChanged(nameof(ScannedAtText));
                    OnPropertyChanged(nameof(FlagTitle));
                    OnPropertyChanged(nameof(FlagColor));
                    OnPropertyChanged(nameof(FlagBackgroundColor));
                    OnPropertyChanged(nameof(CanSave));
                    OnPropertyChanged(nameof(CanDelete));
                }
            }
        }

        public string EditText
        {
            get => editText;
            set
            {
                if (SetProperty(ref editText, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public ScanFlag SelectedFlag
        {
            get => selectedFlag;
            set
            {
                if (SetProperty(ref selectedFlag, value))
                {
                    OnPropertyChanged(nameof(FlagTitle));
                    OnPropertyChanged(nameof(FlagColor));
                    OnPropertyChanged(nameof(FlagBackgroundColor));
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
                    OnPropertyChanged(nameof(CanDelete));
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

        public bool HasScan => Scan is not null;

        public bool HasImage => Scan?.HasImage == true;

        public ImageSource? ScanImage => Scan?.ImageSource;

        public string ScannedAtText => Scan?.ScannedAtText ?? string.Empty;

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public bool CanSave => !IsBusy && Scan is not null && !string.IsNullOrWhiteSpace(EditText);

        public bool CanDelete => !IsBusy && Scan is not null;

        public string FlagTitle => SelectedFlag switch
        {
            ScanFlag.AllergenDetected => "⚠ Allergen detected",
            ScanFlag.MayContain => "⚠ May contain allergen",
            _ => "✓ No allergens detected"
        };

        public Color FlagColor => SelectedFlag switch
        {
            ScanFlag.AllergenDetected => Color.FromArgb("#B42318"),
            ScanFlag.MayContain => Color.FromArgb("#B8860B"),
            _ => Color.FromArgb("#067647")
        };

        public Color FlagBackgroundColor => SelectedFlag switch
        {
            ScanFlag.AllergenDetected => Color.FromArgb("#FEE4E2"),
            ScanFlag.MayContain => Color.FromArgb("#FFF8DC"),
            _ => Color.FromArgb("#D1FADF")
        };

        public IReadOnlyList<ScanFlag> AvailableFlags { get; } =
            new[] { ScanFlag.NoAllergen, ScanFlag.AllergenDetected, ScanFlag.MayContain };

        public ICommand SaveCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand CancelCommand { get; }

        public void Receive(ScanSelectedMessage message)
        {
            Scan = message.Value;
            EditText = message.Value.Text;
            SelectedFlag = message.Value.Flag;
            StatusMessage = string.Empty;
        }

        private async Task SaveAsync()
        {
            if (!CanSave || Scan is null)
            {
                return;
            }

            IsBusy = true;
            StatusMessage = "Saving changes…";

            try
            {
                var updated = await scanDataService.UpdateAsync(
                    Scan.Id,
                    EditText.Trim(),
                    SelectedFlag);

                Scan = updated;
                WeakReferenceMessenger.Default.Send(new RefreshScansMessage());
                await navigationService.GoBackAsync();
            }
            catch (ApiException exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync("Could not save scan", exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = "Could not save the scan.";
                await dialogService.ShowAlertAsync("Could not save scan", exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteAsync()
        {
            if (Scan is null || IsBusy)
            {
                return;
            }

            var confirmed = await dialogService.ConfirmAsync(
                "Delete scan",
                "Remove this scan from your history?",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            IsBusy = true;

            try
            {
                await scanDataService.DeleteAsync(Scan.Id);
                WeakReferenceMessenger.Default.Send(new RefreshScansMessage());
                await navigationService.GoBackAsync();
            }
            catch (ApiException exception)
            {
                StatusMessage = exception.Message;
                await dialogService.ShowAlertAsync("Could not delete scan", exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = "Could not delete the scan.";
                await dialogService.ShowAlertAsync("Could not delete scan", exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

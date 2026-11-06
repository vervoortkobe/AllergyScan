using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AllergyScan.Localization;
using AllergyScan.Interfaces;
using AllergyScan.Messages;
using AllergyScan.Models;
using AllergyScan.Services;

namespace AllergyScan.ViewModels
{
    /// <summary>View model for viewing and editing a single scan.</summary>
    public class ScanDetailsViewModel : ObservableObject, IScanDetailsViewModel
    {
        private readonly IScanDataService scanDataService;
        private readonly IAllergyDataService allergyDataService;
        private readonly IAllergenDetectionService detectionService;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private ScanHistory? scan;
        private int? scanId;
        private string editText = string.Empty;
        private ScanFlag selectedFlag;
        private bool isBusy;
        private string statusMessage = string.Empty;
        private string detectedAllergens = string.Empty;

        public ScanDetailsViewModel(
            IScanDataService scanDataService,
            IAllergyDataService allergyDataService,
            IAllergenDetectionService detectionService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            this.scanDataService = scanDataService;
            this.allergyDataService = allergyDataService;
            this.detectionService = detectionService;
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
            CancelCommand = new AsyncRelayCommand(navigationService.GoBackAsync);
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

        public string DetectedAllergens
        {
            get => detectedAllergens;
            private set
            {
                if (SetProperty(ref detectedAllergens, value))
                {
                    OnPropertyChanged(nameof(HasDetectedAllergens));
                }
            }
        }

        public bool HasScan => Scan is not null;

        public bool HasDetectedAllergens => !string.IsNullOrWhiteSpace(detectedAllergens);

        public bool HasImage => Scan?.HasImage == true;

        public ImageSource? ScanImage => Scan?.ImageSource;

        public string ScannedAtText => Scan?.ScannedAtText ?? string.Empty;

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public bool CanSave => !IsBusy && Scan is not null && !string.IsNullOrWhiteSpace(EditText);

        public bool CanDelete => !IsBusy && Scan is not null;

        public string FlagTitle => SelectedFlag switch
        {
            ScanFlag.Contains => T("Flag.Contains"),
            ScanFlag.MayContain => T("Flag.MayContain"),
            ScanFlag.MayContainTraces => T("Flag.MayContainTraces"),
            _ => T("Flag.None")
        };

        private static string T(string key) => LocalizationService.Instance[key];

        public Color FlagColor => AllergyScan.Services.FlagColors.TextColor;

        public Color FlagBackgroundColor => SelectedFlag switch
        {
            ScanFlag.Contains => AllergyScan.Services.FlagColors.ContainsBackground,
            ScanFlag.MayContain => AllergyScan.Services.FlagColors.MayContainBackground,
            ScanFlag.MayContainTraces => AllergyScan.Services.FlagColors.MayContainTracesBackground,
            _ => AllergyScan.Services.FlagColors.SafeBackground
        };

        public IReadOnlyList<ScanFlag> AvailableFlags { get; } =
            new[] { ScanFlag.NoAllergen, ScanFlag.Contains, ScanFlag.MayContain, ScanFlag.MayContainTraces };

        public ICommand SaveCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand CancelCommand { get; }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var value)
                && int.TryParse(value?.ToString(), out var id))
            {
                scanId = id;
                await InitializeAsync();
            }
        }

        public async Task InitializeAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (scanId is null)
            {
                StatusMessage = T("ScanDetails.NoScan");
                return;
            }

            if (Scan is not null && Scan.Id == scanId.Value)
            {
                return;
            }

            IsBusy = true;
            StatusMessage = T("ScanDetails.Loading");

            try
            {
                var loaded = await scanDataService.GetByIdAsync(scanId.Value);
                Scan = loaded;
                EditText = loaded.Text;
                SelectedFlag = loaded.Flag;
                StatusMessage = string.Empty;

                try
                {
                    var allergies = await allergyDataService.GetAllAsync();
                    var contains = detectionService.Detect(loaded.Text, string.Empty, allergies);
                    var mayContain = detectionService.DetectMayContain(loaded.Text, string.Empty, allergies);
                    var traces = detectionService.DetectMayContainTraces(loaded.Text, string.Empty, allergies);
                    var names = contains.Concat(mayContain).Concat(traces)
                        .Select(m => m.Allergy.Name)
                        .Distinct(StringComparer.OrdinalIgnoreCase);
                    DetectedAllergens = string.Join(", ", names);
                }
                catch
                {
                    DetectedAllergens = string.Empty;
                }
            }
            catch (Exception exception)
            {
                StatusMessage = T("ScanDetails.LoadFailed");
                await dialogService.ShowAlertAsync(T("ScanDetails.Dialog.LoadFailed.Title"), exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            if (!CanSave || Scan is null)
            {
                return;
            }

            IsBusy = true;
            StatusMessage = T("ScanDetails.Saving");

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
                await dialogService.ShowAlertAsync(T("ScanDetails.Dialog.SaveFailed.Title"), exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = T("ScanDetails.SaveFailed");
                await dialogService.ShowAlertAsync(T("ScanDetails.Dialog.SaveFailed.Title"), exception.Message);
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
                T("ScanDetails.DeleteConfirm.Title"),
                T("ScanDetails.DeleteConfirm.Message"),
                T("Common.Delete"),
                T("Common.Cancel"));

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
                await dialogService.ShowAlertAsync(T("ScanDetails.Dialog.DeleteFailed.Title"), exception.Message);
            }
            catch (Exception exception)
            {
                StatusMessage = T("ScanDetails.DeleteFailed");
                await dialogService.ShowAlertAsync(T("ScanDetails.Dialog.DeleteFailed.Title"), exception.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

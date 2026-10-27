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
    public class HistoryViewModel : ObservableRecipient, IRecipient<RefreshScansMessage>
    {
        private readonly IScanDataService scanDataService;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;

        private ObservableCollection<ScanHistory> scans = new();
        private ScanHistory? selectedScan;
        private bool isBusy;
        private bool isRefreshing;
        private string statusMessage = string.Empty;
        private int currentPage = 1;
        private int totalPages;
        private bool hasMore;

        public HistoryViewModel(
            IScanDataService scanDataService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.scanDataService = scanDataService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
            OpenScanCommand = new AsyncRelayCommand(OpenScanAsync);
            ClearHistoryCommand = new AsyncRelayCommand(ClearHistoryAsync);

            Messenger.Register<HistoryViewModel, RefreshScansMessage>(
                this,
                static (recipient, message) => recipient.Receive(message));
        }

        public ObservableCollection<ScanHistory> Scans
        {
            get => scans;
            set
            {
                if (SetProperty(ref scans, value))
                {
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(HasScans));
                }
            }
        }

        public ScanHistory? SelectedScan
        {
            get => selectedScan;
            set
            {
                if (SetProperty(ref selectedScan, value))
                {
                    OnPropertyChanged(nameof(CanOpenScan));
                }
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            private set => SetProperty(ref isBusy, value);
        }

        public bool IsRefreshing
        {
            get => isRefreshing;
            private set => SetProperty(ref isRefreshing, value);
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

        public bool IsEmpty => Scans.Count == 0;
        public bool HasScans => Scans.Count > 0;

        public bool CanOpenScan => SelectedScan is not null && !IsBusy;

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand OpenScanCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public void Receive(RefreshScansMessage message)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await RefreshAsync();
            });
        }

        public async Task InitializeAsync()
        {
            if (scans.Count == 0)
            {
                await RefreshAsync();
            }
        }

        private async Task RefreshAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            IsRefreshing = true;
            StatusMessage = "Loading history...";

            try
            {
                var result = await scanDataService.GetHistoryAsync(1, 20);
                Scans = new ObservableCollection<ScanHistory>(result.Items);
                currentPage = result.Page;
                totalPages = result.TotalPages;
                hasMore = result.HasMore;
                StatusMessage = string.Empty;
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        private async Task LoadMoreAsync()
        {
            if (IsBusy || !hasMore)
                return;

            IsBusy = true;
            try
            {
                currentPage++;
                var result = await scanDataService.GetHistoryAsync(currentPage, 20);
                foreach (var scan in result.Items)
                {
                    Scans.Add(scan);
                }
                totalPages = result.TotalPages;
                hasMore = result.HasMore;
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(HasScans));
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

        private async Task OpenScanAsync()
        {
            if (!CanOpenScan || SelectedScan is null)
                return;

            var scan = SelectedScan;
            SelectedScan = null;

            WeakReferenceMessenger.Default.Send(new ScanSelectedMessage(scan));
            await navigationService.NavigateToScanDetailsAsync();
        }

        private async Task ClearHistoryAsync()
        {
            if (scans.Count == 0)
                return;

            var confirmed = await dialogService.ConfirmAsync(
                "Clear History",
                "Delete all scan history? This cannot be undone.",
                "Delete All",
                "Cancel");

            if (!confirmed)
                return;

            IsBusy = true;
            try
            {
                await scanDataService.DeleteAllAsync();
                Scans.Clear();
                currentPage = 1;
                totalPages = 0;
                hasMore = false;
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(HasScans));
                StatusMessage = "History cleared.";
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

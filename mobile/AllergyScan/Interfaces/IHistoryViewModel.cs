using AllergyScan.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AllergyScan.Interfaces
{
    public interface IHistoryViewModel
    {
        ObservableCollection<ScanHistory> Scans { get; set; }
        ScanHistory? SelectedScan { get; set; }
        bool IsBusy { get; }
        bool IsRefreshing { get; }
        string StatusMessage { get; }
        bool IsEmpty { get; }
        bool HasScans { get; }
        bool CanOpenScan { get; }
        bool HasStatusMessage { get; }

        ICommand RefreshCommand { get; }
        ICommand LoadMoreCommand { get; }
        ICommand OpenScanCommand { get; }
        ICommand ClearHistoryCommand { get; }

        Task InitializeAsync();
    }
}

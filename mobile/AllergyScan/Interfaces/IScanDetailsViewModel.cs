using AllergyScan.Models;
using System.Windows.Input;

namespace AllergyScan.Interfaces
{
    public interface IScanDetailsViewModel
    {
        ScanHistory? Scan { get; }
        string EditText { get; set; }
        ScanFlag SelectedFlag { get; set; }
        bool IsBusy { get; }
        string StatusMessage { get; }
        bool HasScan { get; }
        bool HasImage { get; }
        ImageSource? ScanImage { get; }
        string ScannedAtText { get; }
        bool HasStatusMessage { get; }
        bool CanSave { get; }
        bool CanDelete { get; }
        string FlagTitle { get; }
        Color FlagColor { get; }
        Color FlagBackgroundColor { get; }

        ICommand SaveCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand CancelCommand { get; }

        void ApplyQueryAttributes(IDictionary<string, object> query);
        Task InitializeAsync();
    }
}

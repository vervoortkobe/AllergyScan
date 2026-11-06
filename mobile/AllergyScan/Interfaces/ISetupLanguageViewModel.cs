using AllergyScan.Models;
using CommunityToolkit.Mvvm.Input;

namespace AllergyScan.Interfaces
{
    public interface ISetupLanguageViewModel
    {
        List<Language> Languages { get; set; }
        Language? SelectedLanguage { get; set; }
        bool IsBusy { get; }
        string StatusMessage { get; }
        bool HasStatusMessage { get; }
        bool CanNext { get; }

        IAsyncRelayCommand NextCommand { get; }

        Task InitializeAsync();
    }
}

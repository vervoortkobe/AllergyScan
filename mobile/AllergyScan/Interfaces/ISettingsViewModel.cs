using AllergyScan.Models;
using System.Windows.Input;

namespace AllergyScan.Interfaces
{
    public interface ISettingsViewModel
    {
        List<Language> Languages { get; set; }
        Language? SelectedLanguage { get; set; }
        Language? CurrentLanguage { get; }
        string CurrentLanguageText { get; }
        bool IsBusy { get; }
        string StatusMessage { get; }
        bool HasStatusMessage { get; }
        bool CanSave { get; }

        ICommand SaveCommand { get; }
        ICommand RefreshCommand { get; }

        Task InitializeAsync();
    }
}

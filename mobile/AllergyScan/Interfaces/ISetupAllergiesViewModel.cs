using AllergyScan.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AllergyScan.Interfaces
{
    public interface ISetupAllergiesViewModel
    {
        ObservableCollection<Allergy> Allergies { get; set; }
        string NewAllergyName { get; set; }
        string CountText { get; }
        bool IsBusy { get; }
        string StatusMessage { get; }
        bool HasStatusMessage { get; }
        bool CanAdd { get; }
        bool CanComplete { get; }

        ICommand AddCommand { get; }
        ICommand AddSuggestionCommand { get; }
        IAsyncRelayCommand<Allergy> RemoveAllergyCommand { get; }
        IAsyncRelayCommand<Allergy> ToggleFlagCommand { get; }
        IAsyncRelayCommand CompleteSetupCommand { get; }

        Task InitializeAsync();
        void AddSuggestion(string? suggestion);
        Task ToggleFlagAsync(Allergy? allergy);
    }
}

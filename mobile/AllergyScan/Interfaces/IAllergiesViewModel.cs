using AllergyScan.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AllergyScan.Interfaces
{
    public interface IAllergiesViewModel
    {
        ObservableCollection<Allergy> Allergies { get; set; }
        Allergy? SelectedAllergy { get; set; }
        string NewAllergyName { get; set; }
        bool IsBusy { get; }
        string StatusMessage { get; }
        string CountText { get; }
        bool CanAdd { get; }
        bool CanDeleteSelected { get; }
        bool HasStatusMessage { get; }

        ICommand AddCommand { get; }
        ICommand DeleteSelectedCommand { get; }
        ICommand AddSuggestionCommand { get; }
        ICommand RemoveAllergyCommand { get; }
        ICommand ToggleFlagCommand { get; }

        Task InitializeAsync();
        void AddSuggestion(string? suggestion);
        void RemoveAllergy(Allergy allergy);
        void ToggleFlag(Allergy allergy);
    }
}

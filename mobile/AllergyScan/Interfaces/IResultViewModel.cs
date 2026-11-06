using AllergyScan.Models;
using CommunityToolkit.Mvvm.Input;

namespace AllergyScan.Interfaces
{
    public interface IResultViewModel
    {
        int ScanId { get; set; }
        ScanFlag Flag { get; }
        string Allergens { get; }
        bool HasAllergens { get; }
        bool IsAllergen { get; }
        bool IsMayContain { get; }
        bool IsSafe { get; }
        string ResultTitle { get; }
        string ResultSubtitle { get; }
        Color ResultColor { get; }
        Color ResultBgColor { get; }
        string ResultIcon { get; }

        IAsyncRelayCommand BackCommand { get; }

        void ApplyResult(ScanFlag flag, string allergens);
    }
}

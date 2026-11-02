using AllergyScan.Models;
using AllergyScan.Services;
using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;

namespace AllergyScan.Views
{
    [QueryProperty(nameof(Flag), "flag")]
    [QueryProperty(nameof(Allergens), "allergens")]
    public class ResultPage : ContentPage
    {
        private readonly ResultViewModel viewModel;

        public string Flag { get; set; } = string.Empty;
        public string Allergens { get; set; } = string.Empty;

        public ResultPage(ResultViewModel viewModel)
        {
            this.viewModel = viewModel;
            BindingContext = viewModel;

            if (Enum.TryParse<ScanFlag>(Flag, out var parsedFlag))
            {
                viewModel.ApplyResult(parsedFlag, Allergens);
            }
        }
    }
}

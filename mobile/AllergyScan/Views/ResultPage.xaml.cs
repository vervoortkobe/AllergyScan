using AllergyScan.Models;
using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;

namespace AllergyScan.Views
{
    [QueryProperty(nameof(Flag), "flag")]
    [QueryProperty(nameof(Allergens), "allergens")]
    public class ResultPage : ContentPage
    {
        private readonly ResultViewModel viewModel;
        private bool resultApplied;

        public string Flag { get; set; } = string.Empty;
        public string Allergens { get; set; } = string.Empty;

        public ResultPage(ResultViewModel viewModel)
        {
            this.viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Query properties are set after the constructor, so the result can only
            // be applied once the page is about to appear.
            if (resultApplied || !Enum.TryParse<ScanFlag>(Flag, out var parsedFlag))
            {
                return;
            }

            resultApplied = true;
            viewModel.ApplyResult(parsedFlag, Allergens);
        }
    }
}

using AllergyScan.Models;
using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;

namespace AllergyScan.Views
{
    [QueryProperty(nameof(Flag), "flag")]
    [QueryProperty(nameof(Allergens), "allergens")]
    [QueryProperty(nameof(ScanId), "scanId")]
    public partial class ResultPage : ContentPage
    {
        private readonly ResultViewModel viewModel;
        private bool resultApplied;

        public string Flag { get; set; } = string.Empty;
        public string Allergens { get; set; } = string.Empty;
        public string ScanId { get; set; } = string.Empty;

        public ResultPage(ResultViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (resultApplied)
            {
                return;
            }

            if (Enum.TryParse<ScanFlag>(Flag, out var parsedFlag))
            {
                viewModel.ApplyResult(parsedFlag, Allergens);
            }

            if (int.TryParse(ScanId, out var id) && id > 0)
            {
                viewModel.ScanId = id;
            }

            resultApplied = true;
        }

        protected override bool OnBackButtonPressed()
        {
            if (int.TryParse(ScanId, out var id) && id > 0)
            {
                _ = Shell.Current.GoToAsync("//scan");
                return true;
            }

            _ = Shell.Current.GoToAsync("//scan");
            return true;
        }
    }
}

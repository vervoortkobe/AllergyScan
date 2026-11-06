using AllergyScan.Interfaces;
using AllergyScan.Models;
using AllergyScan.Services;
using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    [QueryProperty(nameof(Flag), "flag")]
    [QueryProperty(nameof(Allergens), "allergens")]
    [QueryProperty(nameof(ScanId), "scanId")]
    public partial class ResultPage : ContentPage
    {
        private readonly IResultViewModel viewModel;
        private readonly INavigationService navigationService;
        private bool resultApplied;

        public string Flag { get; set; } = string.Empty;
        public string Allergens { get; set; } = string.Empty;
        public string ScanId { get; set; } = string.Empty;

        public ResultPage(IResultViewModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.navigationService = navigationService;
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
                _ = navigationService.NavigateToAsync("//scan");
                return true;
            }

            _ = navigationService.NavigateToAsync("//scan");
            return true;
        }
    }
}

using AllergyScan.Interfaces;
using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class ScanPage : ContentPage
    {
        private readonly IScanViewModel viewModel;

        public ScanPage(IScanViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            this.viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.InitializeAsync();
        }
    }
}

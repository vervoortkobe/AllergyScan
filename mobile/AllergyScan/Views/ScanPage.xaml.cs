using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class ScanPage : ContentPage
    {
        private readonly ScanViewModel viewModel;

        public ScanPage(ScanViewModel viewModel)
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

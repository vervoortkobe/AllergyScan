using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class ScanDetailsPage : ContentPage, IQueryAttributable
    {
        private readonly ScanDetailsViewModel viewModel;

        public ScanDetailsPage(ScanDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            this.viewModel = viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            viewModel.ApplyQueryAttributes(query);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.InitializeAsync();
        }
    }
}

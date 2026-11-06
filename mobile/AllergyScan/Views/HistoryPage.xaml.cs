using AllergyScan.Interfaces;
using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class HistoryPage : ContentPage
    {
        private readonly IHistoryViewModel viewModel;

        public HistoryPage(IHistoryViewModel viewModel)
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

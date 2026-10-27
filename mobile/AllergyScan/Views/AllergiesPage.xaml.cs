using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class AllergiesPage : ContentPage
    {
        private readonly AllergiesViewModel viewModel;

        public AllergiesPage(AllergiesViewModel viewModel)
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

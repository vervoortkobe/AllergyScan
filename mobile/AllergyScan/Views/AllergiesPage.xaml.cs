using AllergyScan.Interfaces;
using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class AllergiesPage : ContentPage
    {
        private readonly IAllergiesViewModel viewModel;

        public AllergiesPage(IAllergiesViewModel viewModel)
        {
            this.viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.InitializeAsync();
        }
    }
}
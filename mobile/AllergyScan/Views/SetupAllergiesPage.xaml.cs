using AllergyScan.Interfaces;
using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class SetupAllergiesPage : ContentPage
    {
        private readonly ISetupAllergiesViewModel viewModel;

        public SetupAllergiesPage(ISetupAllergiesViewModel viewModel)
        {
            this.viewModel = viewModel;
            BindingContext = viewModel;

            // Setup is a one-way wizard: hide the navigation bar and tab bar so the
            // user cannot skip a step or leave the setup early.
            Shell.SetNavBarIsVisible(this, false);
            Shell.SetTabBarIsVisible(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.InitializeAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // Do not allow the hardware back button to skip the setup.
            return true;
        }
    }
}
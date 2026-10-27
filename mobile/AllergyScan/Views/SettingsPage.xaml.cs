using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel viewModel;

        public SettingsPage(SettingsViewModel viewModel)
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

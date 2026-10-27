using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class SetupPage : ContentPage
    {
        private readonly SetupViewModel viewModel;

        public SetupPage(SetupViewModel viewModel)
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

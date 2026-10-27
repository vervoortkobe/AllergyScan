using AllergyScan.ViewModels;

namespace AllergyScan.Views
{
    public partial class ScanDetailsPage : ContentPage
    {
        public ScanDetailsPage(ScanDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;

namespace AllergyScan.Views
{
    public partial class SetupPage : ContentPage
    {
        public SetupPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Shell.Current.GoToAsync("//settings");
        }
    }
}

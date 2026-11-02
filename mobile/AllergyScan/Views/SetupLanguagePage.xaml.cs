using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;

namespace AllergyScan.Views
{
    public class SetupLanguagePage : ContentPage
    {
        public SetupLanguagePage(SetupLanguageViewModel viewModel)
        {
            BindingContext = viewModel;

            var resources = Application.Current.Resources;

            var title = new Label
            {
                Text = "Setup",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold
            };

            var subtitle = new Label
            {
                Text = "Choose the language the app translates ingredient labels into.",
                FontSize = 13,
                TextColor = (Color)resources["Gray500"]
            };

            var pickerLabel = new Label
            {
                Text = "Translation language",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            };

            var picker = new Picker { Title = "Choose a language" };
            picker.SetBinding(Picker.ItemsSourceProperty, "Languages");
            picker.ItemDisplayBinding = new Binding("DisplayName");
            picker.SetBinding(Picker.SelectedItemProperty, "SelectedLanguage");

            var next = new Button
            {
                Text = "Next",
                FontSize = 16,
                CornerRadius = 8,
                MinimumHeightRequest = 50,
                TextColor = Colors.White,
                BackgroundColor = (Color)resources["Primary"]
            };
            next.SetBinding(Button.CommandProperty, "NextCommand");
            next.SetBinding(Button.IsEnabledProperty, "CanNext");

            var status = new Label { FontSize = 13, TextColor = (Color)resources["Gray600"] };
            status.SetBinding(Label.TextProperty, "StatusMessage");
            status.SetBinding(Label.IsVisibleProperty, "HasStatusMessage");

            var content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 12,
                Children =
                {
                    title,
                    subtitle,
                    pickerLabel,
                    picker,
                    next,
                    status
                }
            };

            Content = new ScrollView { Content = content };
        }
    }
}

using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace AllergyScan.Views
{
    public class SettingsPage : ContentPage
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            BindingContext = viewModel;

            var resources = Application.Current.Resources;

            var title = new Label
            {
                Text = "Settings",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold
            };

            var languageLabel = new Label
            {
                Text = "Translation language",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            };

            var description = new Label
            {
                Text = "The app translates every ingredients list to this language before checking your allergies.",
                FontSize = 13,
                TextColor = (Color)resources["Gray500"]
            };

            var currentLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)resources["Primary"]
            };
            currentLabel.SetBinding(Label.TextProperty, new Binding("CurrentLanguageText", stringFormat: "Current: {0}"));

            var picker = new Picker { Title = "Choose a language" };
            picker.SetBinding(Picker.ItemsSourceProperty, "Languages");
            picker.ItemDisplayBinding = new Binding("DisplayName");
            picker.SetBinding(Picker.SelectedItemProperty, "SelectedLanguage");

            var saveBtn = new Button { Text = "Save language", FontSize = 14 };
            saveBtn.SetBinding(Button.CommandProperty, "SaveCommand");
            saveBtn.SetBinding(Button.IsEnabledProperty, "CanSave");

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
                    new Border
                    {
                        StrokeShape = new RoundRectangle { CornerRadius = 12 },
                        StrokeThickness = 1,
                        Padding = 16,
                        Content = new VerticalStackLayout
                        {
                            Spacing = 10,
                            Children = { languageLabel, description, currentLabel, picker, saveBtn, status }
                        }
                    }
                }
            };

            Content = new ScrollView { Content = content };
        }
    }
}

using AllergyScan.Interfaces;
using AllergyScan.Localization;
using Microsoft.Maui.Controls.Shapes;

namespace AllergyScan.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly ISettingsViewModel viewModel;

        public SettingsPage(ISettingsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = viewModel;

            var resources = Application.Current.Resources;
            var localization = LocalizationService.Instance;

            var title = new Label
            {
                Text = localization["Settings.Title"],
                FontSize = 22,
                FontAttributes = FontAttributes.Bold
            };

            var languageLabel = new Label
            {
                Text = localization["Settings.LanguageLabel"],
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            };

            var description = new Label
            {
                Text = localization["Settings.Description"],
                FontSize = 13,
                TextColor = (Color)resources["Gray500"]
            };

            var currentLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)resources["Primary"]
            };
            currentLabel.SetBinding(
                Label.TextProperty,
                new Binding("CurrentLanguageText", stringFormat: localization["Settings.Current"]));

            var picker = new Picker { Title = localization["Settings.ChooseLanguage"] };
            picker.SetBinding(Picker.ItemsSourceProperty, "Languages");
            picker.ItemDisplayBinding = new Binding("LocalizedDisplayName");
            picker.SetBinding(Picker.SelectedItemProperty, "SelectedLanguage");

            var saveBtn = new Button { Text = localization["Settings.Save"], FontSize = 14 };
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.InitializeAsync();
        }
    }
}

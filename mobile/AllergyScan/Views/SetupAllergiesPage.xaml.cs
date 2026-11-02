using AllergyScan.Models;
using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace AllergyScan.Views
{
    public class SetupAllergiesPage : ContentPage
    {
        private readonly SetupAllergiesViewModel viewModel;

        public SetupAllergiesPage(SetupAllergiesViewModel viewModel)
        {
            this.viewModel = viewModel;
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
                Text = "Select your allergies and how strictly they should be flagged.",
                FontSize = 13,
                TextColor = (Color)resources["Gray500"]
            };

            var count = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)resources["Gray500"]
            };
            count.SetBinding(Label.TextProperty, "CountText");

            var list = new VerticalStackLayout();
            list.SetBinding(BindableLayout.ItemsSourceProperty, "Allergies");
            BindableLayout.SetItemTemplate(list, new DataTemplate(() =>
            {
                var border = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    StrokeThickness = 1,
                    Padding = new Thickness(10, 6),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                border.SetBinding(Border.StrokeProperty, "FlagStrokeColor");
                border.SetBinding(Border.BackgroundColorProperty, "FlagBackgroundColor");

                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) =>
                {
                    if (s is Border b && b.BindingContext is Allergy a)
                    {
                        await viewModel.ToggleFlagAsync(a);
                    }
                };
                border.GestureRecognizers.Add(tap);

                var nameLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center
                };
                nameLabel.SetBinding(Label.TextProperty, "DisplayName");
                nameLabel.SetBinding(Label.TextColorProperty, "FlagTextColor");

                var badge = new Label
                {
                    FontSize = 9,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(6, 0, 0, 0)
                };
                badge.SetBinding(Label.TextProperty, "FlagBadge");

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    Children = { nameLabel, badge }
                };
                Grid.SetColumn(badge, 1);
                border.Content = grid;
                return border;
            }));

            var complete = new Button
            {
                Text = "Complete setup",
                FontSize = 16,
                CornerRadius = 8,
                MinimumHeightRequest = 50,
                TextColor = Colors.White,
                BackgroundColor = (Color)resources["Primary"]
            };
            complete.SetBinding(Button.CommandProperty, "CompleteSetupCommand");
            complete.SetBinding(Button.IsEnabledProperty, "CanComplete");

            var status = new Label { FontSize = 13, TextColor = (Color)resources["Gray600"] };
            status.SetBinding(Label.TextProperty, "StatusMessage");
            status.SetBinding(Label.IsVisibleProperty, "HasStatusMessage");

            var content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 12,
                Children = { title, subtitle, count, list, complete, status }
            };

            Content = new ScrollView { Content = content };
        }
    }
}

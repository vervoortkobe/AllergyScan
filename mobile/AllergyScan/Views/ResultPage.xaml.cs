using AllergyScan.ViewModels;
using AllergyScan.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace AllergyScan.Views
{
    public class ResultPage : ContentPage
    {
        public ResultPage(ResultViewModel viewModel)
        {
            BindingContext = viewModel;

            var icon = new Label { FontSize = 80, HorizontalOptions = LayoutOptions.Center };
            icon.SetBinding(Label.TextProperty, nameof(ResultViewModel.ResultIcon));
            icon.SetBinding(Label.TextColorProperty, nameof(ResultViewModel.ResultColor));

            var title = new Label
            {
                FontSize = 26,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            };
            title.SetBinding(Label.TextProperty, nameof(ResultViewModel.ResultTitle));
            title.SetBinding(Label.TextColorProperty, nameof(ResultViewModel.ResultColor));

            var subtitle = new Label
            {
                FontSize = 15,
                HorizontalTextAlignment = TextAlignment.Center,
                MaxLines = 3,
                LineBreakMode = LineBreakMode.WordWrap,
                TextColor = Color.FromArgb("#6E6E6E")
            };
            subtitle.SetBinding(Label.TextProperty, nameof(ResultViewModel.ResultSubtitle));

            var allergens = new Label
            {
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                MaxLines = 4,
                LineBreakMode = LineBreakMode.WordWrap
            };
            allergens.SetBinding(Label.TextProperty, nameof(ResultViewModel.Allergens));
            allergens.SetBinding(Label.TextColorProperty, nameof(ResultViewModel.ResultColor));
            allergens.SetBinding(IsVisibleProperty, nameof(ResultViewModel.HasAllergens));

            var border = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                StrokeThickness = 1,
                Padding = 14,
                Margin = new Thickness(0, 12, 0, 0),
                Content = allergens
            };
            border.SetBinding(Border.StrokeProperty, nameof(ResultViewModel.ResultColor));

            var back = new Button
            {
                Text = "Back",
                FontSize = 16,
                CornerRadius = 8,
                MinimumHeightRequest = 50,
                TextColor = Colors.White
            };
            back.SetBinding(Button.CommandProperty, nameof(ResultViewModel.BackCommand));
            back.SetBinding(Button.BackgroundColorProperty, nameof(ResultViewModel.ResultColor));

            var content = new VerticalStackLayout
            {
                Padding = 30,
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                Children = { icon, title, subtitle, border, back }
            };

            Content = new Grid
            {
                RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Star) },
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star) },
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { content }
            };
            Content.SetBinding(Grid.BackgroundColorProperty, nameof(ResultViewModel.ResultBgColor));
        }
    }
}

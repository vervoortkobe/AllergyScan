using AllergyScan.Models;
using AllergyScan.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace AllergyScan.Views
{
    public class AllergiesPage : ContentPage
    {
        private readonly AllergiesViewModel viewModel;

        public AllergiesPage(AllergiesViewModel viewModel)
        {
            this.viewModel = viewModel;
            BindingContext = viewModel;

            var resources = Application.Current.Resources;

            var title = new Label
            {
                Text = "Your allergy profile",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold
            };

            var description = new Label
            {
                Text = "Add every ingredient you must avoid. The scanner compares each product with this list.",
                FontSize = 13,
                TextColor = (Color)resources["Gray500"]
            };

            var suggestionsLabel = new Label
            {
                Text = "Common allergens",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 8, 0, 4)
            };

            var commonButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 6
            };
            foreach (var suggestion in new[] { "Milk", "Egg", "Peanut", "Soy", "Gluten", "Wheat", "Fish", "Shellfish", "Tree nut", "Mustard", "Sesame", "Celery" })
            {
                var btn = new Button
                {
                    Text = suggestion,
                    FontSize = 12,
                    Padding = new Thickness(10, 6),
                    CornerRadius = 16,
                    Margin = new Thickness(0, 0, 6, 6),
                    BackgroundColor = (Color)resources["Primary"],
                    TextColor = Colors.White
                };
                var s = suggestion;
                btn.Clicked += (_, _) => viewModel.AddSuggestion(s);
                commonButtons.Children.Add(btn);
            }

            var entry = new Entry { Placeholder = "For example: milk, egg, peanut" };
            entry.SetBinding(Entry.TextProperty, "NewAllergyName");
            entry.SetBinding(Entry.ReturnCommandProperty, "AddCommand");

            var addBtn = new Button { Text = "Add", FontSize = 12 };
            addBtn.SetBinding(Button.CommandProperty, "AddCommand");
            addBtn.SetBinding(Button.IsEnabledProperty, "CanAdd");

            var input = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 8,
                Children = { entry, addBtn }
            };
            Grid.SetColumn(addBtn, 1);

            var status = new Label { FontSize = 13, TextColor = (Color)resources["Gray600"] };
            status.SetBinding(Label.TextProperty, "StatusMessage");
            status.SetBinding(Label.IsVisibleProperty, "HasStatusMessage");

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
                tap.Tapped += (s, e) =>
                {
                    if (s is Border b && b.BindingContext is Allergy a)
                    {
                        viewModel.ToggleFlag(a);
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

                var del = new Button
                {
                    Text = "✕",
                    FontSize = 12,
                    BackgroundColor = Colors.Transparent,
                    TextColor = Colors.Gray,
                    Padding = 4
                };
                del.Clicked += (s, e) =>
                {
                    if (s is Button b && b.BindingContext is Allergy a)
                    {
                        viewModel.RemoveAllergy(a);
                    }
                };

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    Children = { nameLabel, badge, del }
                };
                Grid.SetColumn(badge, 1);
                Grid.SetColumn(del, 2);
                border.Content = grid;
                return border;
            }));

            var content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 12,
                Children = { title, description, suggestionsLabel, commonButtons, input, status, count, list }
            };

            Content = new ScrollView { Content = content };
        }
    }
}

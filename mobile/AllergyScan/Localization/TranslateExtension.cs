using Microsoft.Maui.Controls.Xaml;

namespace AllergyScan.Localization
{
    /// <summary>
    /// Resolves a translation key in XAML, for example Text="{loc:Translate Scan.Heading}".
    /// The value is resolved when the page is created, so pages are rebuilt when the
    /// user changes the language.
    /// </summary>
    [ContentProperty(nameof(Key))]
    public sealed class TranslateExtension : IMarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return LocalizationService.Instance[Key];
        }
    }
}

using System.Globalization;
using AllergyScan.Models;

namespace AllergyScan.Localization
{
    /// <summary>Shows a <see cref="ScanFlag"/> in the picker with its translated label.</summary>
    public sealed class ScanFlagToTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not ScanFlag flag)
            {
                return string.Empty;
            }

            var key = flag switch
            {
                ScanFlag.Contains => "Flag.Option.Contains",
                ScanFlag.MayContain => "Flag.Option.MayContain",
                ScanFlag.MayContainTraces => "Flag.Option.MayContainTraces",
                _ => "Flag.Option.None"
            };

            return LocalizationService.Instance[key];
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

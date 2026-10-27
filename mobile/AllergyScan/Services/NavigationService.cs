using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace AllergyScan.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task NavigateToAsync(string route, IDictionary<string, object> parameters);
        Task NavigateToScanDetailsAsync();
        Task GoBackAsync();
    }

    public class NavigationService : INavigationService
    {
        public async Task NavigateToAsync(string route)
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(route);
            }
        }

        public async Task NavigateToAsync(string route, IDictionary<string, object> parameters)
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(GetRouteWithParameters(route, parameters));
            }
        }

        public async Task NavigateToScanDetailsAsync()
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("scanDetails");
            }
        }

        public async Task GoBackAsync()
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        private static string GetRouteWithParameters(string route, IDictionary<string, object> parameters)
        {
            var queryStrings = parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value?.ToString() ?? string.Empty)}");
            var queryString = string.Join("&", queryStrings);
            return $"{route}?{queryString}";
        }
    }
}

namespace AllergyScan.Services
{
    /// <summary>
    /// Abstraction over the MAUI navigation stack so view models never talk to
    /// Shelldirectly.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>Navigates to a Shell route.</summary>
        Task NavigateToAsync(string route);

        /// <summary>Navigates to a Shell route with query parameters.</summary>
        Task NavigateToAsync(string route, IDictionary<string, object> parameters);

        /// <summary>Opens the details page for the scan with the given id.</summary>
        Task NavigateToScanDetailsAsync(int scanId);

        /// <summary>Navigates back using the Shell route.</summary>
        Task GoBackAsync();

        /// <summary>Pushes the details page onto the navigation stack.</summary>
        Task NavigateToDetailsPageAsync();

        /// <summary>Pops the current page from the navigation stack.</summary>
        Task NavigateBackAsync();
    }

    /// <summary>
    /// The INavigationService resolve pages through the IServiceProvider and 
    /// drives the navigation stack exposed by Shell.Current.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private INavigation _navigation;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _navigation = Shell.Current?.Navigation;
        }

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

        public async Task NavigateToScanDetailsAsync(int scanId)
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync($"scanDetails?id={scanId}");
            }
        }

        public async Task GoBackAsync()
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        public async Task NavigateToDetailsPageAsync()
        {
            if (_navigation is not null)
            {
                await _navigation.PushAsync(_serviceProvider.GetRequiredService<Views.ScanDetailsPage>());
            }
        }

        public async Task NavigateBackAsync()
        {
            if (_navigation is not null && _navigation.NavigationStack.Count > 1)
            {
                await _navigation.PopAsync();
            }
            else
            {
                throw new InvalidOperationException("No pages to navigate back to!");
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
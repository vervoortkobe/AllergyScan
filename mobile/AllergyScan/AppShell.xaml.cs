using AllergyScan.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AllergyScan
{
    public partial class AppShell : Shell
    {
        private readonly IAllergyDataService allergyDataService;
        private readonly ILanguageDataService languageDataService;
        private readonly IServiceProvider serviceProvider;
        private bool hasCheckedStartup;
        private static bool routesRegistered = false;

        public AppShell(
            IServiceProvider serviceProvider,
            IAllergyDataService allergyDataService,
            ILanguageDataService languageDataService)
        {
            InitializeComponent();
            this.serviceProvider = serviceProvider;
            this.allergyDataService = allergyDataService;
            this.languageDataService = languageDataService;
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Only register routes once to avoid duplicate route errors
            if (routesRegistered)
                return;
            routesRegistered = true;

            Routing.RegisterRoute("scanDetails", new ServiceRouteFactory<Views.ScanDetailsPage>(serviceProvider));
            Routing.RegisterRoute("setup", new ServiceRouteFactory<Views.SetupLanguagePage>(serviceProvider));
            Routing.RegisterRoute("setup-allergies", new ServiceRouteFactory<Views.SetupAllergiesPage>(serviceProvider));
            Routing.RegisterRoute("result", new ServiceRouteFactory<Views.ResultPage>(serviceProvider));
        }

        private sealed class ServiceRouteFactory<TPage>(IServiceProvider serviceProvider) : RouteFactory
            where TPage : Element
        {
            public override Element GetOrCreate() => serviceProvider.GetRequiredService<TPage>();

            public override Element GetOrCreate(IServiceProvider services) => (services ?? serviceProvider).GetRequiredService<TPage>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (hasCheckedStartup)
                return;

            hasCheckedStartup = true;
            await CheckInitialSetupAsync();
        }

        public async Task CheckInitialSetupAsync()
        {
            bool setupDone = Preferences.Default.Get("HasCompletedInitialSetup", false);
            bool langSet = Preferences.Default.Get("LanguageConfigured", false);

            if (!setupDone || !langSet)
            {
                await GoToAsync("setup");
                return;
            }

            try
            {
                var allergies = await allergyDataService.GetAllAsync();
                if (allergies.Count == 0)
                {
                    await GoToAsync("setup");
                }
            }
            catch
            {
                // If offline, continue without blocking
            }
        }
    }
}
